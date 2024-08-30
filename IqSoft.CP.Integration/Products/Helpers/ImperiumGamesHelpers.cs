using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.ImperiumGames;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Products.Helpers
{
	public class ImperiumGamesHelpers
	{
		public static string GetUrl(string token, int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session, ILog log)
		{
			var product = CacheManager.GetProductById(productId);
			var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
			if (provider == null || provider.Name != Constants.GameProviders.ImperiumGames)
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
			var client = CacheManager.GetClientById(clientId);
			var hallId = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.ImperiumGamesHallId);
			var hallKey = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.ImperiumGamesHallKey);
			var input = new
			{
				hall = hallId,
				key = hallKey,
				login = token,
				gameId = product.ExternalId,
				cmd = "openGame",
				demo = isForDemo ? "0" : "1",
				continent = client.CurrencyId,
				domain = session.Domain,
				exitUrl = session.Domain,
				language = session.LanguageId
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				Url = $"{provider.GameLaunchUrl}openGame/",
				PostData = JsonConvert.SerializeObject(input)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<LaunchOutput>(res);
			if (output.status == "success")
			{
                var game = JsonConvert.DeserializeObject<LaunchUrl>(JsonConvert.SerializeObject(output.content.game));
				return game.url;
			}
			throw new Exception(output.error);
		}

		public static List<Game> GetGames(int partnerId, int providerId, ILog log)
		{
			var provider = CacheManager.GetGameProviderById(providerId);
			if (provider == null || provider.Name != Constants.GameProviders.ImperiumGames)
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
			var hallId = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.ImperiumGamesHallId);
			var hallKey = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.ImperiumGamesHallKey);
			var input = new
			{
				hall = hallId,
				key = hallKey,
				cmd = "gamesList"
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				Url = provider.GameLaunchUrl,
				PostData = JsonConvert.SerializeObject(input)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<LaunchOutput>(res);
			if (output.status == "success")
			{
				var games = JsonConvert.DeserializeObject<List<Game>>(JsonConvert.SerializeObject(output.content.gameList));
				var category = games.GroupBy(x => x.categories).Select(t => t.Key).ToList();
				return games;
			}
			throw new Exception(output.error);
		}
	}
}