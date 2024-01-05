using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.MasterCacheWebApiCore;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class EveryMatrixHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.EveryMatrix);
        public static string GetUrl(int clientId, string token, int partnerId, int productId, bool isForDemo, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixOperatorId);
            var externalId = product.ExternalId.Split(',');
            var currentSession = string.Empty;
            if (product.ExternalId == "sports-betting" || product.ExternalId=="live-sports" || product.ExternalId=="esports")
                currentSession = Integration.Products.Helpers.EveryMatrixHelpers.GetOddsMatrixUrl(token, partnerId, isForDemo, session);
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

            string res;
            if (isForDemo)
            {
                var demoInput = new
                {
                    language = session.LanguageId,
                    funMode = true,
                    casinolobbyurl = casinoPageUrl
                };
                if (product.ExternalId=="live-sports" || product.ExternalId=="esports")
                {
                    var launchUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixSportLaunchUrl);
                    res = string.Format("{0}/{1}/{2}{3}{4}?{5}", launchUrl, session.LanguageId, product.ExternalId == "esports" ? "sport/" : string.Empty,
                    externalId[0], product.ExternalId == "esports" ? "/96/all/0/discipline/live/" : string.Empty, CommonFunctions.GetUriEndocingFromObject(demoInput));
                }
                else if (product.ExternalId == "sports-betting")
                    res = string.Format("{0}?{1}", CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixSportLaunchUrl),
                        CommonFunctions.GetUriEndocingFromObject(demoInput));
                else
                    res = string.Format("{0}/Loader/Start/{1}/{2}?{3}", Provider.GameLaunchUrl, operatorId, externalId[0],
                                                                        CommonFunctions.GetUriEndocingFromObject(demoInput));
            }
            else
            {
                Integration.Products.Helpers.EveryMatrixHelpers.RegisterPlayerForCEDirect(clientId, session, Program.DbLogger);
                var input = new
                {
                    _sid = token,
                    language = session.LanguageId,
                    funMode = false,
                    casinolobbyurl = casinoPageUrl
                };
                if (product.ExternalId=="live-sports" || product.ExternalId=="esports")
                {
                    var launchUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixSportLaunchUrl);
                    res = string.Format("{0}/{1}/{2}{3}{4}?{5}", launchUrl, session.LanguageId, product.ExternalId == "esports" ? "sport/" : string.Empty,
                    externalId[0], product.ExternalId == "esports" ? "/96/all/0/discipline/live/" : string.Empty, CommonFunctions.GetUriEndocingFromObject(input));
                }
                else if (product.ExternalId == "sports-betting")
                    res = string.Format("{0}?{1}", CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixSportLaunchUrl),
                        CommonFunctions.GetUriEndocingFromObject(input));
                else
                    res = string.Format("{0}/Loader/Start/{1}/{2}?{3}", Provider.GameLaunchUrl, operatorId, externalId[0],
                                                                        CommonFunctions.GetUriEndocingFromObject(input));
            }
            if (!string.IsNullOrEmpty(currentSession))
                res += "&currentSession=" + currentSession;
            if (externalId.Length == 1)
                return res;
            return string.Format("{0}&tableID={1}", res, externalId[1]);
        }
    }
}