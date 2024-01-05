using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using  IqSoft.CP.Integration.Products.Models.SoftSwiss;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class SoftSwissHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.SoftSwiss);
        public static List<string> UnsuppordedCurrenies = new List<string>
        {
          Constants.Currencies.IranianTuman
        };
        public static string GetUrl(int partnerId, int productId, int clientId, bool isForDemo, bool isForMobile, SessionIdentity session, string token)
        {
            var product = CacheManager.GetProductById(productId);
            var client = CacheManager.GetClientById(clientId);
            if (!isForDemo && client==null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
            var partner = CacheManager.GetPartnerById(partnerId);
            var casinoId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.SoftSwissCasinoId);
            var authToken = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.SoftSwissAuthToken);
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

            var createSessionInput = new
            {
                casino_id = casinoId,
                game = product.ExternalId,
                currency,
                locale = session.LanguageId,
                ip = session.LoginIp,
                client_type = isForMobile ? "mobile" : "desktop",
                urls = new
                {
                    return_url = casinoPageUrl,
                    deposit_url = cashierPageUrl,
                },
                user = new
                {
                    id = isForDemo ? null : client?.Id.ToString(),
                    firstname = client?.Id.ToString(),
                    lastname = client?.UserName,
                    nickname = client?.UserName
                }
            };
            var body = JsonConvert.SerializeObject(createSessionInput, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var hashString = CommonFunctions.ComputeHMACSha256(body, authToken);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = HttpMethod.Post,
                Url = string.Format("{0}/{1}", Provider.GameLaunchUrl, isForDemo ? "demo" : "sessions"),
                RequestHeaders = new Dictionary<string, string> { { "X-REQUEST-SIGN", hashString.ToLower() } },
                PostData = body
            };
            return JsonConvert.DeserializeObject<OpenGameOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).LaunchParameters.GameUrl;
        }

        public static void AddFreeRound(int clientId, int bonusId, List<string> productExternalIds, int spinCount, DateTime finishTime)
        {
            var client = CacheManager.GetClientById(clientId);
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var casinoId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.SoftSwissCasinoId);
            var authToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.SoftSwissAuthToken);
            var currency = client != null ? client.CurrencyId : partner.CurrencyId;
            if (UnsuppordedCurrenies.Contains(currency))
                currency = Constants.Currencies.USADollar;
            var input = new
            {
                casino_id = casinoId,
                issue_id = $"{clientId}_{bonusId}",
                currency,
                games = productExternalIds,
                freespins_quantity = spinCount,
                valid_until = finishTime.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture),
                user = new
                {
                    id = client.Id.ToString(),
                    firstname = client?.Id.ToString(),
                    lastname = client?.UserName,
                    nickname = client?.UserName
                }
            };
            var body = JsonConvert.SerializeObject(input, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var hashString = CommonFunctions.ComputeHMACSha256(body, authToken);

            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestHeaders = new Dictionary<string, string> { { "X-REQUEST-SIGN", hashString.ToLower() } },
                Url = $"{Provider.GameLaunchUrl}/freespins/issue",
                PostData = body
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

        public static void CancelFreeRound(int clientId, int bonusId)
        {
            var client = CacheManager.GetClientById(clientId);
            var casinoId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.SoftSwissCasinoId);
            var authToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.SoftSwissAuthToken);
            var input = new
            {
                casino_id = casinoId,
                issue_id = bonusId.ToString()
            };
            var body = JsonConvert.SerializeObject(input, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var hashString = CommonFunctions.ComputeHMACSha256(body, authToken);

            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestHeaders = new Dictionary<string, string> { { "X-REQUEST-SIGN", hashString.ToLower() } },
                Url = $"{Provider.GameLaunchUrl}/freespins/cancel",
                PostData = body
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
    }
}