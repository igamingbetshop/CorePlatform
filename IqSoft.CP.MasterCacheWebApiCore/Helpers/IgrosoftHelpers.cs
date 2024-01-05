using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.MasterCacheWebApiCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public class IgrosoftHelpers
    {
        class IgrosoftSession
        {
            [JsonProperty(PropertyName = "game")]
            public string GameCode { get; set; }

            [JsonProperty(PropertyName = "uuidToken")]
            public string Token { get; set; }

            [JsonProperty(PropertyName = "demo")]
            public decimal? Demo { get; set; }

            [JsonProperty(PropertyName = "currency")]
            public string Currency { get; set; }

            [JsonProperty(PropertyName = "payout")]
            public int Payout { get; set; }

            [JsonProperty(PropertyName = "userId")]
            public string ClientId { get; set; }

            [JsonProperty(PropertyName = "makeTransaction")]
            public string CallbackUrl { get; set; }
        }

        class SessionLaunch
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "token")]
            public string Token { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "server")]
            public string Server { get; set; }

            [JsonProperty(PropertyName = "launch")]
            public string Launch { get; set; }

            [JsonProperty(PropertyName = "userId")]
            public int ClientId { get; set; }
        }
        public static string GetUrl(int partnerId, int productId, int clientId, string token, string language, bool isDemo, SessionIdentity identity, out string externalToken)
        {
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.Igrosoft);
            var salt = CacheManager.GetGameProviderValueByKey(partnerId, gameProvider.Id, Constants.PartnerKeys.IgrosoftSalt);
            var merchantId = CacheManager.GetGameProviderValueByKey(partnerId, gameProvider.Id, Constants.PartnerKeys.IgrosoftMerchantIId);
            BllClient client=null;
            if (!isDemo)
            {
                client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(language, Constants.Errors.ClientNotFound);
            }
            var product = CacheManager.GetProductById(productId);
            var partner = CacheManager.GetPartnerById(partnerId);

            using (var partnerBl = new PartnerBll(identity, Program.DbLogger))
            {
                if (isDemo)
                    token = CommonFunctions.GetRandomString(10);
                var igrosoftSession = new IgrosoftSession
                {
                    GameCode = product.ExternalId,
                    Token = "1",
                    Currency = client != null ? client.CurrencyId : partner.CurrencyId,
                    Payout = 3,
                    ClientId = client != null ? client.Id.ToString() : string.Empty,
                    CallbackUrl = string.Format("{0}/{1}/{2}", partnerBl.GetPaymentValueByKey(partnerId, null, Constants.PartnerKeys.ProductGateway), partnerId,
                                                                                                            "api/Igrosoft/ApiRequest")
                };
                if (isDemo)
                    igrosoftSession.Demo = 100M;
                var timestamp = CommonFunctions.GetCurrentUnixTimestampMillis().ToString();
                Dictionary<string, string> requestHeaders = new Dictionary<string, string>
                {
                    { "X-Casino-Merchant-Id", merchantId},
                    { "X-Casino-Transaction-Id", token },
                    { "X-Casino-Timestamp", timestamp }
                };

                var sign = CommonFunctions.ComputeMd5(
                    requestHeaders["X-Casino-Merchant-Id"] +
                    requestHeaders["X-Casino-Transaction-Id"] +
                    requestHeaders["X-Casino-Timestamp"] +
                    salt).ToLower();
                requestHeaders.Add("X-Casino-Signature", sign);
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    Url = gameProvider.GameLaunchUrl + "session_create",
                    RequestHeaders = new Dictionary<string, string>(requestHeaders),
                    PostData = JsonConvert.SerializeObject(igrosoftSession,
                                                           new JsonSerializerSettings()
                                                           {
                                                               NullValueHandling = NullValueHandling.Ignore
                                                           })
                };

                var resp = JsonConvert.DeserializeObject<SessionLaunch>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                externalToken = resp.Token;
                return string.Format(resp.Launch + "?token={0}&language={1}", resp.Token, language);
            }
        }
		/*
        public static string GetProductsList()
        {
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.Igrosoft);
            var salt = CacheManager.GetGameProviderValueByKey(1, gameProvider.Id, Constants.PartnerKeys.IgrosoftSalt);
            var merchantId = CacheManager.GetGameProviderValueByKey(1, gameProvider.Id, Constants.PartnerKeys.IgrosoftMerchantIId);
            var timestamp = CommonFunctions.GetCurrentUnixTimestampMillis().ToString();
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>
                {
                    { "X-Casino-Merchant-Id", merchantId},
                    { "X-Casino-Transaction-Id", "123" },
                    { "X-Casino-Timestamp", timestamp }
                };

            var sign = CommonFunctions.ComputeMd5(
                requestHeaders["X-Casino-Merchant-Id"] +
                requestHeaders["X-Casino-Transaction-Id"] +
                requestHeaders["X-Casino-Timestamp"] +
                salt).ToLower();
            requestHeaders.Add("X-Casino-Signature", sign);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Get,
                Url = "https://math-server.net/icasino2/games",
                RequestHeaders = new Dictionary<string, string>(requestHeaders)
            };
            return CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
		*/
    }
}