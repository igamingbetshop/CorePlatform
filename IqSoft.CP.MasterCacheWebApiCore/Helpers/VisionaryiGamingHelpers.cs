using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApiCore.Helpers
{
    public static class VisionaryiGamingHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.VisionaryiGaming);

        public static string GetUrl(string token, int partnerId, SessionIdentity session)
        {
            var siteId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.VisionarySiteId);
            var input = new
            {
                OTP = token,
                siteID = siteId,
                language = session.LanguageId
            };
            return string.Format("{0}?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(input));
        }
    }
}