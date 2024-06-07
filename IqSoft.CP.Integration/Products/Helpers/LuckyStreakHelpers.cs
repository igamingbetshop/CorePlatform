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
using System.Text;
using System.Text.RegularExpressions;

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
			var details = product.ExternalId.Split(','); 
			var launchUrl = string.Empty;
			var isProviderGame = details[0] == "providerGame";
			if (!isForDemo)
			{
				var param = isProviderGame ? $"authCode={token}&operatorName={operatorName}&playerName={client.Id}&additionalParams={token}"
										   : $"additionalParams={token}&PlayerName={client.Id}&OperatorName={operatorName}&AuthCode={token}&GameId={details[1]}&GameType={details[2]}";
				launchUrl = isProviderGame ? $"{provider.GameLaunchUrl}/providergateway/{details[2]}/api/game/play?{details[3]}&{param}"
											   : $"{provider.GameLaunchUrl}?{param}";
			}
			else
				launchUrl = $"{provider.GameLaunchUrl}/providergateway/{details[2]}/api/game/play?{details[3]}&mode=demo";
			return launchUrl;
		}

		public static List<Product> GetGames(int partnerId, int providerId, ILog log)
		{
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
			var providerIds = new Dictionary<int, string>()
			{
				{3, "PragmaticPlay" },
				{8, "Spinomenal" },
				{9, "Fugaso" },
				{13, "Yggdrasil" }
			};
			var key = 0;
			foreach (var pg in providerGames)
			{
				try
				{ 
					var match = Regex.Match(pg.launchUrl, @"providergateway/([^/]+)/");
					var queryString = pg.launchUrl.Split('?')[1];
					key = pg.providerId;
					var product = new Product
					{
						id = pg.id,
						name = pg.name,
						provider = providerIds[key],
						externalId = $"providerGame,{pg.id},{match.Groups[1].Value},{queryString}",  //providerName,gameId(for callbacks)
						type = pg.type,
						imageUrl = pg.dealer?.avatarUrl,
						demoUrl = pg.demoUrl
					};
					products.Add(product);
				}
				catch (KeyNotFoundException ex)
				{
					log.Error($"Key not found: {key}");
				}
				continue;
			}
			var lobbyGames = GetGames(token, $"{provider.GameLaunchUrl}/lobby/api/v4/lobby/games");
			foreach (var lg in lobbyGames)
			{
					var match = Regex.Match(lg.launchUrl, @"GameId=(\d+)&GameType=([^&]+)");
					var product = new Product
					{
						id = lg.id,
						name = lg.name,
						provider = "LuckyStreak",
						externalId = $"lobbyGame,{match.Groups[1].Value},{match.Groups[2].Value}",  // gameId,gameType
						type = lg.type,
						imageUrl = lg.dealer?.avatarUrl,
						demoUrl = null
					};
					products.Add(product);
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

		public static string GetWidgetURL(int partnerId, int providerId, string timestamp, int clientId)
		{
			var widgetURL = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.SmarticoWidgetURL);
			var saltKey = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.SmarticoSaltKey);
			var brandKey = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.SmarticoBrandKey);
			var labelKey = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.SmarticoLabelKey);
			var hash = CommonFunctions.ComputeMd5($"{clientId}:{saltKey}:{timestamp}".ToLower());
			return $"{widgetURL}?label_key={labelKey}&brand_key={brandKey}&user_ext_id={clientId}&user_hash={hash}:{timestamp}";
		}
	}
}