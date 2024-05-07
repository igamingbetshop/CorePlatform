using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.RiseUp;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IqSoft.CP.Integration.Products.Helpers
{
	public class RiseUpHelpers
	{
		public static string GetUrl(string token, int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session, ILog log)
		{
			var product = CacheManager.GetProductById(productId);
			var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
			if (provider == null || provider.Name != Constants.GameProviders.RiseUp)
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
			var client = CacheManager.GetClientById(clientId);
			var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.RiseUpOperatorId);
			var pin = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.RiseUpApiKey);
			var game = product.ExternalId.Split(',');
			var data = new
			{
				userID = client.Id.ToString(),
				username = client.UserName,
				gameID = game[0],
				token = token,
				currency = client.CurrencyId,
				language = session.LanguageId,
				mode = isForDemo ? "demo" : "real",
				device = session.DeviceType == (int)DeviceTypes.Desktop ? "desktop" : "mobile"
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{operatorId}:{pin}")) } },
				Url = $"{provider.GameLaunchUrl}/{game[1].Replace("/"," ")}/session/new",
				PostData = JsonConvert.SerializeObject(data)
			};
			var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<BaseOutput>(response);
			if (output.Status)
			{
				var url = JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(output.Data));
				return url;
			}
			throw new Exception($"Error: {output.Error} ");
		}				

		public static List<Product> GetGames(int partnerId, int providerId)
		{
			var provider = CacheManager.GetGameProviderById(providerId);
			if (provider == null || provider.Name != Constants.GameProviders.RiseUp)
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);

			var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.RiseUpOperatorId);
			var pin = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.RiseUpApiKey);
			var httpRequestInput = new HttpRequestInput
			{
				RequestMethod = Constants.HttpRequestMethods.Get,
				RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{operatorId}:{pin}")) } },
				Url = $"{provider.GameLaunchUrl}/games/{operatorId}"
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<BaseOutput>(res);
			if (output.Status)
			{
				var games = JsonConvert.DeserializeObject<List<Games>>(JsonConvert.SerializeObject(output.Data));
				var products = new List<Product>();
				foreach ( var g in games)
				{
					var group = g.GameList.GroupBy(x => x.Id);
					foreach (var item in group)
					{
						var product = new Product
						{
							id = item.FirstOrDefault().Id,
							name = item.FirstOrDefault().Name,
							description = item.FirstOrDefault().Description,
							provider = item.FirstOrDefault().Provider,
							externalId = $"{item.FirstOrDefault().Id},{item.FirstOrDefault().Provider.Replace(" ","/")}",
							type = item.FirstOrDefault().Type,
							webImageUrl = item.FirstOrDefault(x => x.Device == "desktop")?.ImgUrl,
							mobileImgUrl = item.FirstOrDefault(x => x.Device == "mobile")?.ImgUrl,
						};
						products.Add(product);
					}

				}
				return products;
			}
			throw new Exception($"Error: {output.Error} ");
		}
	}
}
