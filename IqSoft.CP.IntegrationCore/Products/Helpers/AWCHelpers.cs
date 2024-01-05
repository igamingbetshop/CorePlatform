using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Products.Models.AWC;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class AWCHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.AWC);
        public static readonly Dictionary<string, List<int>> BetLimits = new Dictionary<string, List<int>>
        {
            { Constants.Currencies.Euro, new List<int>{262201, 262203, 262204, 262205, 262206, 262207} },
            { Constants.Currencies.USADollar, new List<int>{260701/*,260702,260703,260704,260705,260706 */} },
            { Constants.Currencies.MyanmarKyat, new List<int>{262501/*, 262502, 262503, 262504, 262505 */} },
            { Constants.Currencies.CambodianRiel, new List<int>{ 263703/* 263701, 263702, 263703*/ } },
            { Constants.Currencies.LaotianKip, new List<int>{264501/*, 264502, 264503*/ } }
        };
    
        public static string GetUrl(int partnerId, int productId, int clientId, string token, bool isForDemo, bool isForMobile, SessionIdentity session)
        {
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.AWCSecureKey);
            var agentId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.AWCAgentId);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = Provider.GameLaunchUrl
            };
            var product = CacheManager.GetProductById(productId);
            var externalData = product.ExternalId.Split('|');
            if (!isForDemo)
            {
                var betLimit = new BetLimit //THB
                {
                    Platform = new PlatformType
                    {
                        Type = new LimitType { LimitId = new List<int> {260933/* 260901, 260902, 260903, 260904, 260905, 260906*/ } }
                    }
                };
                
                var client = CacheManager.GetClientById(clientId);
                if (BetLimits.ContainsKey(client.CurrencyId))
                    betLimit.Platform.Type.LimitId = BetLimits[client.CurrencyId];
                var createPlayerInput = new
                {
                    cert = apiKey,
                    agentId,
                    userId = client.Id,
                    currency = client.CurrencyId,
                    betLimit = JsonConvert.SerializeObject(betLimit),
                    language = session.LanguageId
                };
                httpRequestInput.Url = string.Format("{0}/wallet/createMember", Provider.GameLaunchUrl);
                httpRequestInput.PostData = CommonFunctions.GetUriDataFromObject(createPlayerInput);
               var userOutput = JsonConvert.DeserializeObject<LaunchOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if(userOutput.status == "1001")
                {
                    var updateLimitsInput = new
                    {
                        cert = apiKey,
                        agentId,
                        userId = client.Id,
                        betLimit = JsonConvert.SerializeObject(betLimit)
                    };
                    httpRequestInput.Url = string.Format("{0}/wallet/updateBetLimit", Provider.GameLaunchUrl);
                    httpRequestInput.PostData = CommonFunctions.GetUriDataFromObject(updateLimitsInput);
                    CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                }
                else if (userOutput.status != "0000" && userOutput.status != "1001")
                  return string.Format("Code: {0}, Message: {1}", userOutput.status, userOutput.desc);
                var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
                if (string.IsNullOrEmpty(casinoPageUrl))
                    casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
                else
                    casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

                var launchInput = new
                {
                    cert = apiKey,
                    agentId,
                    userId = client.Id,
                    gameCode = externalData[0],
                    gameType = externalData[1],
                    platform = externalData[2],
                    isMobileLogin = isForMobile,
                    extension1 = token,
                    externalURL = casinoPageUrl,
                    language = session.LanguageId
                };
                httpRequestInput.Url = string.Format("{0}/wallet/doLoginAndLaunchGame", Provider.GameLaunchUrl);
                httpRequestInput.PostData = CommonFunctions.GetUriEndocingFromObject(launchInput);
                var output = JsonConvert.DeserializeObject<LaunchOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (output.status == "0000")
                    return output.url;
                return output.status;
            }
            return string.Empty;
        }
    }
}
