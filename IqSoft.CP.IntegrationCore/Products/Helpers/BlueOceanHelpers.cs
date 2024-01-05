using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Products.Models.BlueOcean;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static IqSoft.CP.Common.Constants;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class BlueOceanHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.BlueOcean);
        public static List<string> UnsuppordedCurrenies = new List<string>
        {
            Currencies.IranianTuman,
            Currencies.IranianRial
        };
        public static string GetUrl(int partnerId, int productId, int clientId, bool isForDemo, SessionIdentity session, ILog log, out string token)
        {
            token = string.Empty;
            var product = CacheManager.GetProductById(productId);
            var client = CacheManager.GetClientById(clientId);
            var partner = CacheManager.GetPartnerById(partnerId);
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BlueOceanApiKey);
            var pwd = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BlueOceanApiPwd);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = Provider.GameLaunchUrl
            };
            var currency = client != null ? client.CurrencyId : partner.CurrencyId;

            if (UnsuppordedCurrenies.Contains(currency))
                currency = Constants.Currencies.USADollar;
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

            var cashierPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashierPageUrl).StringValue;
            if (string.IsNullOrEmpty(cashierPageUrl))
                cashierPageUrl = string.Format("https://{0}/user/1/deposit", session.Domain);
            else
                cashierPageUrl = string.Format(cashierPageUrl, session.Domain);

            if (!isForDemo)
            {
                if (client == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.NotAllowed);
                else
                {
                    var createPlayerInput = new
                    {
                        api_password = pwd,
                        api_login = apiKey,
                        method = "createPlayer",
                        lang = session.LanguageId,
                        user_id = client.Id,
                        user_username = client.Id,
                        user_password = client.Id,
                        currency
                    };
                    httpRequestInput.PostData = CommonFunctions.GetUriEndocingFromObject(createPlayerInput);
                    CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                }
                var input = new
                {
                    api_password = pwd,
                    api_login = apiKey,
                    method = "getGame",
                    lang = session.LanguageId,
                    user_id = client?.Id,
                    user_username = client.Id,
                    user_password = client.Id,
                    gameid = product.ExternalId,
                    homeurl = casinoPageUrl,
                    play_for_fun = 0,
                    currency,
                };
                httpRequestInput.PostData = CommonFunctions.GetUriEndocingFromObject(input);
            }
            else
            {
                var input = new
                {
                    api_password = pwd,
                    api_login = apiKey,
                    method = "getGame",
                    lang = session.LanguageId,
                    gameid = product.ExternalId,
                    homeurl = casinoPageUrl,
                    cashierurl = cashierPageUrl,
                    play_for_fun = 1,
                    currency,
                };
                httpRequestInput.PostData = CommonFunctions.GetUriEndocingFromObject(input);
            }
            var gResult = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var res = JsonConvert.DeserializeObject<OpenGameOutput>(gResult);
            if (res.response == null)
                throw new Exception(string.Format("Code {0}. {1}", res.error, res.message));
            
            token = res.sessionid + "_" + res.gamesession_id;
            return res.response;
        }

        public static List<GameItem> GetProductsList(int partnerId)
        {
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BlueOceanApiKey);
            var pwd = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BlueOceanApiPwd);
            var partner = CacheManager.GetPartnerById(partnerId);
            var input = new
            {
                api_password = pwd,
                api_login = apiKey,
                method = "getGameList",
                currency = partner.CurrencyId,
                show_jackpot_feed = true
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                Url = Provider.GameLaunchUrl,
                PostData = CommonFunctions.GetUriEndocingFromObject(input)
            };
            var res = JsonConvert.DeserializeObject<GamesList>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (res.response == null)
                throw new Exception(string.Format("Code {0}. {1}", res.error, res.message));
            return res.response;
        }

        public static Dictionary<string, List<double>> GetJackpotFeed(int partnerId, ILog log)
        {
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BlueOceanApiKey);
            var pwd = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BlueOceanApiPwd);
            var partner = CacheManager.GetPartnerById(partnerId);
            var input = new
            {
                api_password = pwd,
                api_login = apiKey,
                method = "getGameList",
                currency = partner.CurrencyId,
                show_jackpot_feed = true
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                Url = Provider.GameLaunchUrl,
                PostData = CommonFunctions.GetUriEndocingFromObject(input)
            };
            var res = JsonConvert.DeserializeObject<GamesList>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (res.response == null)
                throw new Exception(string.Format("Code {0}. {1}", res.error, res.message));
            return res.response.Where(x => x.has_jackpot == true && x.jackpotfeed != null).ToDictionary(x => x.id, x => x.jackpotfeed);
        }

        public static void AddFreeRound(int clientId, List<string> productExternalIds, int spinCount, DateTime startTime, DateTime finishTime)
        {
            var client = CacheManager.GetClientById(clientId);
            var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.BlueOceanApiKey);
            var pwd = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.BlueOceanApiPwd);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = Provider.GameLaunchUrl
            };
            var currency = !UnsuppordedCurrenies.Contains(client.CurrencyId) ? client.CurrencyId : Constants.Currencies.USADollar;
            var createPlayerInput = new
            {
                api_password = pwd,
                api_login = apiKey,
                method = "createPlayer",
                lang = "en",
                user_id = clientId,
                user_username = clientId,
                user_password = clientId,
                currency
            };
            httpRequestInput.PostData = CommonFunctions.GetUriEndocingFromObject(createPlayerInput);
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var getPlayerInput = new
            {
                api_password = pwd,
                api_login = apiKey,
                method = "playerExists",
                user_username = clientId,
                currency
            };
            httpRequestInput.PostData = CommonFunctions.GetUriEndocingFromObject(getPlayerInput);
            var playerId = JsonConvert.DeserializeObject<PlayerOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).response.id;
            var input = new
            {
                api_password = pwd,
                api_login = apiKey,
                playerids = playerId,
                gameids = string.Join(",", productExternalIds),
                available = spinCount,
                validTo = finishTime.ToString("yyyy-MM-dd"),
                validFrom = startTime.ToString("yyyy-MM-dd"),
                method = "addFreeRounds",
                currency
            };
            httpRequestInput.PostData = CommonFunctions.GetUriEndocingFromObject(input);
            var res = JsonConvert.DeserializeObject<FreeRoundOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (res.response == null)
                throw new Exception(string.Format("Code {0}. {1}", res.error, res.message));
            //   "response": "{\"created\":1,\"freeround_id\":\"5b8fc08970d5544676009e24\"}", must saved            
        }

        public static void CancelFreeRound(int clientId, string freespinId)
        {
            var client = CacheManager.GetClientById(clientId);
            var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.BlueOceanApiKey);
            var pwd = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.BlueOceanApiPwd);
            var input = new
            {
                api_password = pwd,
                api_login = apiKey,
                playerids = clientId.ToString(),
                method = "removeFreeRounds",
                freeround_id = freespinId, //not saved in iqsoft
                currency = !UnsuppordedCurrenies.Contains(client.CurrencyId) ? client.CurrencyId : Constants.Currencies.USADollar
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                Url = Provider.GameLaunchUrl,
                PostData = CommonFunctions.GetUriEndocingFromObject(input)
            };
            var res = JsonConvert.DeserializeObject<FreeRoundOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (res.response == null)
                throw new Exception(string.Format("Code {0}. {1}", res.error, res.message));
         //   return res.response;
        }

        public static string GetGameReport(int clientId, int? productId, string roundId)
        {
            var client = CacheManager.GetClientById(clientId);
            var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.BlueOceanApiKey);
            var pwd = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.BlueOceanApiPwd);
            var gameId = productId.HasValue ? CacheManager.GetProductById(productId.Value).ExternalId : null;

            var input = new
            {
                api_password = pwd,
                api_login = apiKey,
                method = "getRoundHistory",
                currency = !UnsuppordedCurrenies.Contains(client.CurrencyId) ? client.CurrencyId : Constants.Currencies.USADollar,
                user_username = clientId,
                user_password = clientId,
                game_id = gameId,
                round_id = roundId
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                Url = Provider.GameLaunchUrl,
                PostData = CommonFunctions.GetUriEndocingFromObject(input)
            };
            httpRequestInput.PostData = CommonFunctions.GetUriEndocingFromObject(input);
            var res = JsonConvert.DeserializeObject<GameReportOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (res.response == null)
                throw new Exception(string.Format("Code {0}. {1}", res.error, res.message));
            return res.response;
        }
    }
}