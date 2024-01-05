using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using  IqSoft.CP.Integration.Products.Models.SoftSwiss;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using YamlDotNet.RepresentationModel;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class SoftSwissHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.SoftSwiss);
        public static List<string> UnsuppordedCurrenies = new List<string>
        {
          Constants.Currencies.IranianTuman
        };
        public static string GetUrl(int partnerId, int productId, int clientId, bool isForDemo, bool isForMobile, SessionIdentity session, ILog log)
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
                    nickname = client?.UserName,
                    city = !string.IsNullOrEmpty(client?.City) ? client?.City : session.Country,
                    date_of_birth = client?.BirthDate.ToString("yyyy-MM-dd"),
                    registered_at = client?.CreationTime.ToString("yyyy-MM-dd"),
                    gender = client?.Gender == (int)Gender.Male ? "m" : "f",
                    country = session.Country
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
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = string.Format("{0}/{1}", Provider.GameLaunchUrl, isForDemo ? "demo" : "sessions"),
                RequestHeaders = new Dictionary<string, string> { { "X-REQUEST-SIGN", hashString.ToLower() } },
                PostData = body
            };
            var r = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var gameUrl = JsonConvert.DeserializeObject<Models.SoftSwiss.OpenGame.OpenGameOutput>(r).LaunchParameters.GameUrl;
            if (string.IsNullOrEmpty(gameUrl))
            {
                var gameOtherUrl = JsonConvert.DeserializeObject<Models.SoftSwiss.GameLaunchOther.GameLaunchOtherOutput>(r);
                return isForMobile ? gameOtherUrl.LaunchParameters.MobileUrl : gameOtherUrl.LaunchParameters.DesktopUrl;
            }
            if (string.IsNullOrEmpty(gameUrl))
                log.Debug(r);
            return gameUrl;

        }

        public static void AddFreeRound(int clientId, int bonusId, List<string> productExternalIds, int spinCount, DateTime finishTime, ILog log)
        {
            var requestBody = string.Empty;
            try
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
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestHeaders = new Dictionary<string, string> { { "X-REQUEST-SIGN", hashString.ToLower() } },
                    Url = $"{Provider.GameLaunchUrl}/freespins/issue",
                    PostData = body
                };
                requestBody = JsonConvert.SerializeObject(httpRequestInput);
                CommonFunctions.SendHttpRequest(httpRequestInput, out _);

            }
            catch (Exception ex)
            {
                log.Error("Softswiss_Freespin: " + requestBody + " __Error: " + ex);
                throw;
            }
        }

        public static void CancelFreeRound(int clientId, int bonusId)
        {
            var client = CacheManager.GetClientById(clientId);
            var casinoId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.SoftSwissCasinoId);
            var authToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.SoftSwissAuthToken);
            var input = new
            {
                casino_id = casinoId,
                issue_id = $"{clientId}_{bonusId}",
            };
            var body = JsonConvert.SerializeObject(input, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var hashString = CommonFunctions.ComputeHMACSha256(body, authToken);

            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestHeaders = new Dictionary<string, string> { { "X-REQUEST-SIGN", hashString.ToLower() } },
                Url = $"{Provider.GameLaunchUrl}/freespins/cancel",
                PostData = body
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

        private static string GetProvidersUrls(int partnerId, ILog log)
        {
            var resourcesUrlKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.SoftSwissResourcesUrl).StringValue;
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = $"{resourcesUrlKey}/allprovidersurls.txt"
            };
            log.Debug(httpRequestInput.Url);
            return CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

        public static List<YamlItem> GetGames(int partnerId, ILog log)
        {
            var allProvidersUrlsText = GetProvidersUrls(partnerId, log);
            var allProvidersUrls = allProvidersUrlsText.Split('\n').Select(x => x.Trim('\r')).ToList();
            var result = new List<YamlItem>();
            var resourcesUrlKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.SoftSwissResourcesUrl).StringValue;
            foreach (var providerUrl in allProvidersUrls)
            {
                Uri restUri = new Uri(providerUrl);
                var fileName = providerUrl.Split('/').Last();
                string localTarget = $"C:\\products\\softswiss\\{fileName}";
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(restUri, localTarget);
                }
                var yamlText = File.ReadAllText(localTarget);
                result.AddRange(GetGameItems(yamlText));
            }
            return result;
        }

        private static List<YamlItem> GetGameItems(string yamlText)
        {
            var yamlStream = new YamlStream();
            var result = new List<YamlItem>();
            using (var stringReader = new StringReader(yamlText))
            {
                yamlStream.Load(stringReader);
            }
            var mapping1 = yamlStream.Documents[0].RootNode;
            YamlSequenceNode mapping = (YamlSequenceNode)mapping1;
            var chlids = mapping.Children;
            foreach (var item in chlids)
            {
                bool isMobile = false;
                bool isDesktop = false;
                var devices = item["devices"].ToString();
                devices = devices.Substring(2, devices.Length - 4);
                if (devices.Contains("mobile"))
                    isMobile = true;
                if (devices.Contains("desktop"))
                    isDesktop = true;
                decimal? payout = null;
                try
                {
                    payout = Convert.ToDecimal(item["payout"].ToString());
                }
                catch { }

                result.Add(new YamlItem
                {
                    Title = item["title"].ToString(),
                    Identifier = item["identifier"].ToString(),
                    Identifier2 = item["identifier2"].ToString(),
                    Provider = item["provider"].ToString(),
                    Producer = item["producer"].ToString(),
                    Category = item["category"].ToString(),
                    HasFreespins = Convert.ToBoolean(item["has_freespins"].ToString()),
                    FeatureGroup = item["feature_group"].ToString(),
                    Payout = payout,
                    IsDesktop = isDesktop,
                    IsMobile = isMobile                    
                });
            }
            return result;
        }
    }
}