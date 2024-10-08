using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.Bonus;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.TimelessTech;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Products.Helpers
{
	public class TimelessTechHelpers
	{
        private static readonly List<string> NotSupportedCurrencies = new List<string>
        {
            Constants.Currencies.USDT,
            Constants.Currencies.USDC,
            Constants.Currencies.PYUSD,
            Constants.Currencies.BUSD
        };
        public static string GetUrl(string token, int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session, ILog log)
		{
			var product = CacheManager.GetProductById(productId);
			var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
			if (provider == null || provider.Name != Constants.GameProviders.TimelessTech && provider.Name != Constants.GameProviders.BCWGames)
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);

			var client = CacheManager.GetClientById(clientId);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.TimelessTechOperatorID);
			var device = session.DeviceType == (int)DeviceTypes.Desktop ? "desktop" : "mobile";
			var mode = isForDemo ? "fun" : "real";
			var launchUrl = CacheManager.GetGameProviderValueByKey(partnerId, product.SubProviderId ?? product.GameProviderId.Value, Constants.PartnerKeys.TimelessTechLaunchUrl);
			if (string.IsNullOrEmpty(launchUrl))
				launchUrl = provider.GameLaunchUrl;
			var gameId = product.ExternalId.Contains("lobby") ? $"lobby_id={product.ExternalId.Split(new string[] { "lobby_" }, StringSplitOptions.None)[1]}" : $"game_id={product.ExternalId}";
			var url = $"{launchUrl}/?mode={mode}&{gameId}&language={session.LanguageId}&operator_id={operatorID}&device={device}";
			if (isForDemo)
				return url;
			var currency = client.CurrencyId;
            if (NotSupportedCurrencies.Contains(currency))
                currency = Constants.Currencies.USADollar;

            return $"{url}&currency={currency}&token={token}";
		}

		public static List<Game> GetGames(int partnerId, int providerId)
		{
			var provider = CacheManager.GetGameProviderById(providerId);
            if (provider == null || provider.Name != Constants.GameProviders.TimelessTech && provider.Name != Constants.GameProviders.BCWGames)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
            var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"games{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var httpRequestInput = new HttpRequestInput
			{
				RequestMethod = Constants.HttpRequestMethods.Get,
				RequestHeaders = headers,
				Url = $"{url}/games/list/all"
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var games = JsonConvert.DeserializeObject<Data>(res);
			
			var limits = GetLimits(partnerId, providerId);
			foreach (var game in games.games)
			{
				var items = limits?.Where(x => x.game_id == game.id).Select(x => new { C = x.currency_code, V = x.limits }).ToList();
				var values = new Dictionary<string, List<decimal>>();
                foreach(var item in items)
				{
					if (!values.ContainsKey(item.C))
						values.Add(item.C, new List<decimal>());

					var existing = values[item.C];
					existing.AddRange(item.V);

                    values[item.C] = existing.Distinct().OrderBy(x => x).ToList();
                }

                game.betValue = JsonConvert.SerializeObject(values);
			}

			var lobbies = GetLobbies(partnerId, providerId);
			games.games.AddRange(lobbies);
			return games.games;
		}

		public static List<Game> GetLobbies(int partnerId, int providerId)
		{
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"lobbies{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var httpRequestInput = new HttpRequestInput
			{
				RequestMethod = Constants.HttpRequestMethods.Get,
				RequestHeaders = headers,
				Url = $"{url}/lobbies/list/all"
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var lobbies = JsonConvert.DeserializeObject<Lobbies>(res);
			var games = new List<Game>();
			foreach (var item in lobbies.lobbies)
			{
				var game = new Game
				{
					title = $"lobby_{item.lobby_id}",
					vendor = item.vendor,
					platform = item.platform,
					subtype = item.subtype,
					details = item.details,
				};
				games.Add(game);
			}
			return games;
		}

		public static string GetSignature(int partnerId, int providerId, string timestamp, int clientId, string clientCurrencyId)
		{
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechSecretkey);
			var signature = clientId == 0 ? CommonFunctions.ComputeSha1($"relayer{timestamp}{sectetKey}")
												  : CommonFunctions.ComputeSha1($"relayer{clientId}{clientCurrencyId}{timestamp}{sectetKey}");
			return signature;
		}

		public static bool CreateCampaign(FreeSpinModel freeSpinModel, string providerName, ILog log)
		{
			log.Info("CreateCampaign_" + JsonConvert.SerializeObject(freeSpinModel));
            if (providerName != Constants.GameProviders.TimelessTech && providerName != Constants.GameProviders.BCWGames)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
            var provider = CacheManager.GetGameProviderByName(providerName);

			var client = CacheManager.GetClientById(freeSpinModel.ClientId);
			var sectetKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, provider.Id, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(client.PartnerId, provider.Id, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(client.PartnerId, provider.Id, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"campaigns{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var httpRequestInput = new HttpRequestInput
			{
				RequestMethod = Constants.HttpRequestMethods.Get,
				RequestHeaders = headers,
				Url = $"{url}/campaigns/vendors"
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var vendors = JsonConvert.DeserializeObject<Vendor>(res);
			var product = CacheManager.GetProductByExternalId(provider.Id, freeSpinModel.ProductExternalId);
			if (!product.SubProviderId.HasValue)
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
			var subProvider = CacheManager.GetGameProviderById(product.SubProviderId.Value).Name;
			var vendor = vendors.data.FirstOrDefault(x => subProvider.ToLower().Replace("livegaming", string.Empty).Replace("direc", string.Empty) == x.ToLower().
				Replace("-", string.Empty).Replace(" ", string.Empty) ||
				subProvider.ToLower().Replace("gaming", string.Empty).
				Replace("games", string.Empty) == x.ToLower().Replace("gaming", string.Empty) ||
				subProvider.Length > 5 && x.Length > 5 && subProvider.ToLower().Substring(0, 6) == x.ToLower().
				Replace("-", string.Empty).Replace(" ", string.Empty).Substring(0, 6));

            decimal? bValue = null;
            if (!string.IsNullOrEmpty(freeSpinModel.BetValues))
            {
                var bv = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(freeSpinModel.BetValues);
                if (bv.ContainsKey(client.CurrencyId))
                    bValue = bv[client.CurrencyId];
            }
            if (bValue == null && !string.IsNullOrEmpty(product.BetValues))
            {
                var bv = JsonConvert.DeserializeObject<Dictionary<string, List<decimal>>>(product.BetValues);
                if (bv.ContainsKey(client.CurrencyId) && bv[client.CurrencyId].Any())
                    bValue = bv[client.CurrencyId][0];
            }
			if (bValue == null)
				return false;

            var data = new Campaign
			{
				vendor = vendor,
				campaign_code = freeSpinModel.BonusId.ToString(),
				freespins_per_player = freeSpinModel.SpinCount.Value,
				begins_at = ((DateTimeOffset)DateTime.UtcNow.AddSeconds(10)).ToUnixTimeSeconds(),
				expires_at = ((DateTimeOffset)freeSpinModel.FinishTime).ToUnixTimeSeconds(),
				currency_code = client.CurrencyId,
				players = client.Id.ToString(),
				games = new List<GameModel>
				{
					new GameModel
					{
						game_id = Convert.ToInt32(freeSpinModel.ProductExternalId),
						total_bet = bValue.Value
                    }
				}
			};

			httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = $"{url}/campaigns/create",
				PostData = JsonConvert.SerializeObject(data)
            };
			res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            log.Info("CreateCampaign_Response_" + res);

            var status = JsonConvert.DeserializeObject<GameLimits>(res);
			if (status.status == "OK")
				return true;
			return false;
        }

		public static List<Datum> GetLimits(int partnerId, int providerId)
		{
			var resp = new List<Datum>();
            var partner = CacheManager.GetPartnerById(partnerId);
			var provider = CacheManager.GetGameProviderById(providerId);
			if (provider == null || provider.Name != Constants.GameProviders.TimelessTech && provider.Name != Constants.GameProviders.BCWGames)
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"campaigns{operatorID}{sectetKey}");
			var currencies = CacheManager.GetSupportedCurrencies().Where(x => x.Length <= 3).ToList();
			foreach (var c in currencies)
			{
				var headers = new Dictionary<string, string> {
					{ "X-Authorization", authorization },
					{ "X-Operator-Id", operatorID }
				};
				var httpRequestInput = new HttpRequestInput
				{
					RequestMethod = Constants.HttpRequestMethods.Get,
					RequestHeaders = headers,
					Url = $"{url}/campaigns/vendors/limits?currencies={c}"
				};
				var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var games = JsonConvert.DeserializeObject<GameLimits>(res);
				if (games.data != null)
					resp.AddRange(JsonConvert.DeserializeObject<List<Datum>>(JsonConvert.SerializeObject(games.data)));
			}
			return resp;
		}

		/*
		public static void CancelCampaign(int partnerId, int providerId)
		{
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"campaigns{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var data = new
			{
				campaign_code = "",
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = $"{url}/campaigns/cancel",
				PostData = JsonConvert.SerializeObject(data)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
		}

		public static void AddPlayersToCampaign(int partnerId, int providerId)
		{
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"campaigns{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var data = new
			{
				campaign_code = "",
				players = "",
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = $"{url}/campaigns/players/add",
				PostData = JsonConvert.SerializeObject(data)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
		}

		public static void RemovePlayersFromCampaign(int partnerId)
		{
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"campaigns{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var data = new
			{
				campaign_code = "",
				players = "",
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				RequestHeaders = headers,
				Url = $"{url}/campaigns/players/remove",
				PostData = JsonConvert.SerializeObject(data)
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
		}*/
	}
}
