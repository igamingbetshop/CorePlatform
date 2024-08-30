using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
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
	public static class RiseUpHelpers
	{
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.RiseUp);
        public static string GetUrl(string token, int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session, ILog log)
		{
			var product = CacheManager.GetProductById(productId);
			var client = CacheManager.GetClientById(clientId);
			var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.RiseUpOperatorId);
			var pin = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.RiseUpApiKey);
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
				Url = $"{Provider.GameLaunchUrl}/{game[1].Replace("/"," ")}/session/new",
				PostData = JsonConvert.SerializeObject(data)
			};
			var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<BaseOutput>(response);
			if (output.Status)
				return JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(output.Data));
			throw new Exception($"Error: {output.Error} ");
		}				

		public static List<Product> GetGames(int partnerId, int providerId)
		{
			var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.RiseUpOperatorId);
			var pin = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.RiseUpApiKey);
			var httpRequestInput = new HttpRequestInput
			{
				RequestMethod = Constants.HttpRequestMethods.Get,
				RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{operatorId}:{pin}")) } },
				Url = $"{Provider.GameLaunchUrl}/games/{operatorId}"
			};
			var output = JsonConvert.DeserializeObject<BaseOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
			if(!output.Status)
                throw new Exception($"Error: {output.Error} ");
			return JsonConvert.DeserializeObject<List<Games>>(JsonConvert.SerializeObject(output.Data))
									.SelectMany(x => x.GameList).GroupBy(x => new { x.Id, x.Name, x.Description, x.Type, x.Provider })
									.Select(x => new Product
									{
										Provider = x.Key.Provider,
										Id = x.Key.Id,
										Name = x.Key.Name,
										Description = x.Key.Description,
										Type = x.Key.Type,
										DesktopSupport = x.Any(y => y.Device == "desktop"),
										MobileSupport = x.Any(y => y.Device == "mobile"),
										WebImageUrl = x.FirstOrDefault(y => y.Device == "desktop")?.ImgUrl,
										MobileImageUrl = x.FirstOrDefault(y => y.Device == "mobile")?.ImgUrl
									}).ToList();         
		}
	}
}
