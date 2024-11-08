﻿using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.PragmaticPlay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class PragmaticPlayHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.PragmaticPlay);
        private static readonly List<string> NotSupportedCurrencies = new List<string>
        {
            Constants.Currencies.USDT
        };
        public static string GetSessionUrl(int partnerId, BllProduct product, string token, bool isForMobile, bool isForDemo, SessionIdentity session)
        {
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

            if (isForDemo)
            {
                var partner = CacheManager.GetPartnerById(partnerId);
                var demoInput = new
                {
                    gameSymbol = product.ExternalId,
                    lang = session.LanguageId,
                    cur = NotSupportedCurrencies.Contains(partner.CurrencyId) ? Constants.Currencies.USADollar : partner.CurrencyId,
                    lobbyUrl = casinoPageUrl
                };
                return string.Format("{0}/gs2c/openGame.do?{1}", Provider.GameLaunchUrl,
                                     CommonFunctions.GetUriEndocingFromObject(demoInput));
            }
            var apiUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PragmaticPlayCasinoDomain);
            var secureLogin = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PragmaticPlaySecureLogin);
            var requestInput = new
            {
                token,
                symbol = product.ExternalId,
                technology = "H5",
                platform = isForMobile ? "MOBILE" : "WEB",
                language = session.LanguageId,
                cashierUrl = string.Format("https://{0}/user/1/deposit", session.Domain),
                lobbyUrl = casinoPageUrl
            };
            return string.Format("{0}/gs2c/playGame.do?key={1}&stylename={2}", apiUrl, WebUtility.UrlEncode(CommonFunctions.GetUriDataFromObject(requestInput)), secureLogin);
        }
// var inputString = string.Format("secureLogin={0}&options=GetFrbDetails", secureLogin);
        public static List<GameItem> GetProductsList(int partnerId)
        {
            var domain = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PragmaticPlayCasinoApiUrl);
            var secureLogin = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PragmaticPlaySecureLogin);
            var secureKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PragmaticPlaySecureKey);
            var input = new { secureLogin, options = "GetFrbDetails" };
            var inputString = CommonFunctions.GetSortedParamWithValuesAsString(input, "&");
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                Url = string.Format("{0}/IntegrationService/v3/http/CasinoGameAPI/getCasinoGames/", domain),
                PostData = string.Format("{0}&hash={1}", inputString, CommonFunctions.ComputeMd5(inputString + secureKey))
            };
            var res = JsonConvert.DeserializeObject<GamesOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (res.ErrorCode != 0)
                throw new Exception(string.Format("Code {0}. {1}", res.ErrorCode, res.Description));
            return res.GamesList;
        }

        public static void AddFreeRound(int clientId, List<string> productExternalIds, int spinCount, int clientBonusId, DateTime startTime, DateTime finishTime)
        {
            var client = CacheManager.GetClientById(clientId);
            var apiUrl = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PragmaticPlayCasinoApiUrl);
            var secureLogin = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PragmaticPlaySecureLogin);
            var secureKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PragmaticPlaySecureKey);
            var currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId;
            var input = new
            {
                secureLogin,
                bonusCode = clientBonusId,
                startDate = ((DateTimeOffset)startTime).ToUnixTimeSeconds(),
                expirationDate = ((DateTimeOffset)finishTime).ToUnixTimeSeconds(),
                rounds = spinCount,
                playerId = clientId,
                currency
            };
            var games = productExternalIds.Select(x => new { gameId = x, betValues = new List<object> { new { betPerLine = 0.05, currency } } }).ToList();
            var gameList = new { gameList = games };
            var inputString = CommonFunctions.GetSortedParamWithValuesAsString(input, "&");
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format("{0}/IntegrationService/v3/http/FreeRoundsBonusAPI/v2/bonus/player/create/?{1}", apiUrl,
                      string.Format("{0}&hash={1}", inputString, CommonFunctions.ComputeMd5(inputString + secureKey))),
                PostData = JsonConvert.SerializeObject(gameList)
            };
            var res = JsonConvert.DeserializeObject<BaseOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (res.ErrorCode != 0)
                throw new Exception(string.Format("Code {0}. {1}", res.ErrorCode, res.Description));
        }

        public static void CanceFreeRound(int clientId, int bonusId)
        {
            var client = CacheManager.GetClientById(clientId);
            var apiUrl = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PragmaticPlayCasinoApiUrl);
            var secureLogin = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PragmaticPlaySecureLogin);
            var secureKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.PragmaticPlaySecureKey);

            var input = new
            {
                secureLogin,
                bonusCode = bonusId,
                playerList = new List<int> { clientId }
            };
            var inputString = CommonFunctions.GetSortedParamWithValuesAsString(input, "&");
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format("{0}/IntegrationService/v3/http/FreeRoundsBonusAPI/v2/players/remove/", apiUrl),
                PostData = string.Format("{0}&hash={1}", inputString, CommonFunctions.ComputeMd5(inputString + secureKey))
            };
            var res = JsonConvert.DeserializeObject<BaseOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (res.ErrorCode != 0)
                throw new Exception(string.Format("Code {0}. {1}", res.ErrorCode, res.Description));
        }

    }
}
