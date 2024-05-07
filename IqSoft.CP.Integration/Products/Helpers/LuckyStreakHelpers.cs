using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.LuckyStreak;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IqSoft.CP.Integration.Products.Helpers
{
	public class LuckyStreakHelpers
	{
		public static string GetUrl(string token, int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session, ILog log)
		{
			var product = CacheManager.GetProductById(productId);
			var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
			if (provider == null || provider.Name != Constants.GameProviders.LuckyStreak)
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
			var client = CacheManager.GetClientById(clientId);
			var operatorName = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.LuckyStreakOperatorName);
			var operatorClientName = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.LuckyStreakOperatorClientName);
			var operatorClientSecret = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.LuckyStreakOperatorClientSecret);
			var tokenUrl = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.LuckyStreakTokenUrl);
			var autoCode = GetToken(operatorName, tokenUrl, operatorClientName, operatorClientSecret);
			var baseUrl = product.ExternalId.Split(',');
			var launchUrl = isForDemo ? baseUrl[1] :  baseUrl[1].Contains("providergateway") ?  $"{baseUrl[1]}&authCode={autoCode}&operatorName={operatorName}&userName={client.Id}"
				                                                 : string.Format(baseUrl[1], client.Id, operatorName, autoCode);
			return launchUrl;
		}

		public static List<Product> GetGames(int partnerId, int providerId, ILog log)
		{
			var games = new List<Game>();
			var products = new List<Product>();
			var provider = CacheManager.GetGameProviderById(providerId);
			if (provider == null || provider.Name != Constants.GameProviders.LuckyStreak)
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);

			var url = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.LuckyStreakTokenUrl);
			var operatorName = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.LuckyStreakOperatorName);
			var operatorClientName = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.LuckyStreakOperatorClientName);
			var operatorClientSecret = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.LuckyStreakOperatorClientSecret);
			var token = GetToken(operatorName, url, operatorClientName, operatorClientSecret);
			var providerGames = GetGames(token, $"{provider.GameLaunchUrl}/lobby/api/v4/lobby/providergames");
			var lis = providerGames.GroupBy(x => x.providerId);
			var type = providerGames.GroupBy(x => x.type);
			games.AddRange(providerGames);
			var lobbyGames = GetGames(token, $"{provider.GameLaunchUrl}/lobby/api/v4/lobby/games");
			games.AddRange(lobbyGames);
			var providerIds = new Dictionary<int, string>()
			{
				{0, "LuckyStreak" },
				{3, "PragmaticPlay" },
				{8, "Spinomenal" },
				{9, "Fugaso" }
			};
			var key = 0;
			foreach (var item in games)
			{
				try
				{
					key = item.providerId;
					var product = new Product
					{
						id = item.id,
						name = item.name,
						provider = providerIds[key],
						externalId = $"{item.id},{item.launchUrl},{item.demoUrl}",
						type = item.type,
						imageUrl = item.dealer?.avatarUrl
					};
					products.Add(product);
				}
				catch (KeyNotFoundException ex)
				{
					log.Error($"Key not found: {key}");
				}
				continue;
			}

			return products;
		}

		private static List<Game> GetGames(string token, string url)
		{
			var data = new
			{
				Data = new
				{
					Open = false
				}
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + token } },
				Url = url,
				PostData = JsonConvert.SerializeObject(data)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<GameOutput>(res);
			if (output.errors != null)
			{
				var error = JsonConvert.DeserializeObject<Error>(JsonConvert.SerializeObject(output.errors));
				throw new Exception($"Error: {error.code} {error.title} {error.detail} ");
			}
			var games = JsonConvert.DeserializeObject<Data>(JsonConvert.SerializeObject(output.data));
			return games.games;
		}

		public static string GetToken(string operatorName, string url, string operatorClientName, string operatorClientSecret)
		{
			var data = new
			{
				grant_type = "operator_authorization",
				scope = "operator offline_access",
				operator_name = operatorName
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{operatorClientName}:{operatorClientSecret}")) } },
				Url = url,
				PostData = CommonFunctions.GetUriDataFromObject(data)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<AuthorizationOutput>(res);
			return output.access_token;
		}
	}
}