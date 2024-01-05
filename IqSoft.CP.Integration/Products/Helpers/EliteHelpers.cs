using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using IqSoft.CP.Integration.Products.Models.Elite;
using System.Linq;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class EliteHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.Elite);
        public static string GetUrl(string token, int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session, log4net.ILog log)
        {

            var product = CacheManager.GetProductById(productId);
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);

            var skinCode = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EliteSkinCode);
            var launchUrl = string.Empty;
            if (!isForDemo)
            {
                using (var regionBl = new RegionBll(session, log))
                {
                    var region = regionBl.GetRegionByCountryCode(session.Country);
                    var input = new
                    {
                        username = clientId,
                        sessionToken = token,
                        gameId = product.ExternalId,
                        country = region.IsoCode3,
                        currency = client.CurrencyId,
                        playMode = "R",
                        skinCode = skinCode,
                        platform = session.DeviceType == (int)DeviceTypes.Desktop ? "W" : "M",
                        language = session.LanguageId.ToUpper()
                    };
                    launchUrl = string.Format("{0}?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(input));
                }
            }
            else
            {
                var demoInput = new
                {
                    gameId = product.ExternalId,
                    playMode = "D",
                    skinCode = skinCode,
                    platform = session.DeviceType == (int)DeviceTypes.Desktop ? "W" : "M",
                    language = session.LanguageId.ToUpper()
                };
                launchUrl = string.Format("{0}?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(demoInput));
            }
            return launchUrl;
        }

        public static List<Game> GetGames(int partnerId)
        {
            var skinCode = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EliteSkinCode);
            var authorizationKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EliteAuthorizationKey);
            var apiUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EliteApiUrl);            
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", $"{authorizationKey}" } },
                Url = $"{apiUrl}/gamelist/{skinCode}"
            };
            var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);            
            return JsonConvert.DeserializeObject<List<Game>>(res);
        }
    }
}
