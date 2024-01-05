using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class BetsyHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.Betsy);

        public static string GetUrl( string token, int partnerId, SessionIdentity session)
        {
            var cId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BetsyPartnerId);
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

            var input = new
            {
                locale = session.LanguageId,
                cid = cId,
                token,
                parent = casinoPageUrl
            };
            return string.Format("{0}?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(input));
        }
    }
}