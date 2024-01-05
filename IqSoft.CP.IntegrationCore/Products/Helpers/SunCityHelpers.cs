using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.SunCity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Helpers
{
   public class SunCityHelpers
    {
        private static int gameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.SunCity).Id;

        private static Dictionary<string, string> Currencies { get; set; } = new Dictionary<string, string>
        {
            { "CNY", "rmb" },
            { "IDR", "idr_1000" }
        };

        public static string GenerateBrandAccessToken(int partnerId, string currencyId)
        {
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, gameProviderId, Constants.PartnerKeys.SunCityOperatorID + currencyId);
            var securKey = CacheManager.GetGameProviderValueByKey(partnerId, gameProviderId, Constants.PartnerKeys.SunCitySecureKey + currencyId);
            var url = CacheManager.GetGameProviderValueByKey(partnerId, gameProviderId, Constants.PartnerKeys.SunCityApiUrl);
            var outhInput = new OuthInput
            {
                client_id = operatorId,
                client_secret = securKey,
                grant_type = "client_credentials",
                scope = "playerapi"
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = url+ "/api/oauth/token",
                PostData = CommonFunctions.GetUriEndocingFromObject(outhInput)
            };
            var response = JsonConvert.DeserializeObject<OuthOutput>( CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if(!string.IsNullOrEmpty(response.error))
                throw new ArgumentNullException(response.error_description);

            return response.access_token;
        }

        public static string GeneratePlayerToken(int clientId, SessionIdentity session, bool isForMobile)
        {
            var client = CacheManager.GetClientById(clientId);
           var token = GenerateBrandAccessToken(client.PartnerId, client.CurrencyId);
            var url = CacheManager.GetGameProviderValueByKey(client.PartnerId, gameProviderId, Constants.PartnerKeys.SunCityApiUrl);
            var playerInput = new PlayerInput
            {
                IpAddress = session.LoginIp,
                UserId = clientId.ToString(),
                Username = string.Format("{0} {1}", client.FirstName, client.LastName),
                Tag = null,
                Language = CommonHelpers.LanguageISOCodes.ContainsKey( session.LanguageId)? CommonHelpers.LanguageISOCodes[session.LanguageId] : session.LanguageId,
                Currency = Currencies.ContainsKey(client.CurrencyId) ? Currencies[client.CurrencyId] : client.CurrencyId,
                BetLimitId = 1,
                IsTestPlayer = true,
                PlatformType = isForMobile ? 1 : 0
            };
            Dictionary<string, string> header = new Dictionary<string, string> { { "Authorization", "Bearer " + token } };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = url + "/api/player/authorize",
                PostData = JsonConvert.SerializeObject(playerInput),
                RequestHeaders= header
            };
            var response = JsonConvert.DeserializeObject<PlayerOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
           // var games = GetGamesList(client.PartnerId, true, response.AuthToken, header);
            return response.AuthToken;
        }

        public static string GetGamesList( int partnerId, bool isDesktop, string playerToken, Dictionary<string, string> header)
        {
            var input = new
            {
                lang = "en-US",
                platformtype = isDesktop ? 0 : 1,
                authtoken = playerToken,
                iconres = "343x200"
            };
            var url = CacheManager.GetGameProviderValueByKey(partnerId, gameProviderId, Constants.PartnerKeys.SunCityApiUrl);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = System.Net.Http.HttpMethod.Get,
                Url = url + "/api/games?" + CommonFunctions.GetUriEndocingFromObject(input),
                RequestHeaders = header
            };
            return CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
    }
}
