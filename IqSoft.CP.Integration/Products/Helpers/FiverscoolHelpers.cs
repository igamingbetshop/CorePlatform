using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.Fiverscool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Helpers
{
	public class FiverscoolHelpers
	{
		private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.Fiverscool);
		public static string GetUrl(int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session)
		{
			var agentCode = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.FiverscoolAgentCode);
			var agentToken = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.FiverscoolAgentToken);
			var product = CacheManager.GetProductById(productId);
			var data = new
			{
				method = "game_launch",
				agent_code = agentCode,
				agent_token = agentToken,
				user_code = clientId.ToString(),
				provider_code = product.NickName,
				game_code = product.ExternalId,
				lang = session.LanguageId
			};
			var httpRequestInput = new HttpRequestInput
			{
				RequestMethod = Constants.HttpRequestMethods.Post,
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				Url = Provider.GameLaunchUrl,
				PostData = JsonConvert.SerializeObject(data)
			};
			var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
			if (output.Message == "SUCCESS")
				return output.LaunchUrl;
			else
				throw new Exception($"Error: {output.Message}");
		}


		public static List<GamesWithProvider> GetGames(int partnerId)
		{
			var gamesWithProvider = new List<GamesWithProvider>();
			var agentCode = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.FiverscoolAgentCode);
			var agentToken = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.FiverscoolAgentToken);
			var providerData = new
			{
				method = "provider_list",
				agent_code = agentCode,
				agent_token = agentToken,
			};
			var httpRequestInput = new HttpRequestInput
			{
				RequestMethod = Constants.HttpRequestMethods.Post,
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				Url = Provider.GameLaunchUrl,
				PostData = JsonConvert.SerializeObject(providerData)
			};
			var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var providersOutput = JsonConvert.DeserializeObject<ProviderList>(response);
			var providers = new List<Provider>();
			if (providersOutput.Message == "SUCCESS")
				providers = providersOutput.Providers;
			else
				throw new Exception($"Error Provider: {providersOutput.Message}");
			foreach (var provider in providers)
			{
				var gameData = new
				{
					method = "game_list",
					agent_code = agentCode,
					agent_token = agentToken,
					provider_code = provider.Code
				};
				httpRequestInput = new HttpRequestInput
				{
					RequestMethod = Constants.HttpRequestMethods.Post,
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					Url = Provider.GameLaunchUrl,
					PostData = JsonConvert.SerializeObject(gameData)
				};
				response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var gamesOutput = JsonConvert.DeserializeObject<GameList>(response);
				var games = new List<Game>();
				if (gamesOutput.Message == "SUCCESS")
				{
					gamesWithProvider.Add(new GamesWithProvider { Games = gamesOutput.Games, Provider = provider });
				}
				else
					throw new Exception($"Error Provider: {gamesOutput.Message}");
			}
			return gamesWithProvider;
		}
	}
}
