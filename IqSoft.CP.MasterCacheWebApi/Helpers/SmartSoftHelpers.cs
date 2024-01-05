using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class SmartSoftHelpers
    {
        private static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.SmartSoft);
        public static string GetUrl(string token, int partnerId, int productId, int clientId, bool isForDemo, bool isForMobile, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            var client = CacheManager.GetClientById(clientId);
            var portalName = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.SmartSoftPortalName);
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

            var input = new
            {
                Token = (isForDemo || client == null) ? "DEMO" : token,
                ReturnUrl = casinoPageUrl,
                Lang = session.LanguageId,
                GameCategory = !isForMobile ? product.ExternalId.Split(',')[1] : product.ExternalId.Split(',')[2],
                GameName = product.ExternalId.Split(',')[0],
                PortalName = (isForDemo || client == null) ? "demo" : portalName
            };
            return string.Format("{0}?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(input));
        }
    }
}