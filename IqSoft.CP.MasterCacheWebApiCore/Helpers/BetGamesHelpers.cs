using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class BetGamesHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.BetGames);
        public static string GetUrl(string token, int partnerId, bool isForDemo, bool isForMobile, SessionIdentity session)
        {
            var partnerCode = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BetGamesPartnerCode);
            var partnerServer = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BetGamesPartnerServer);
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

            var input = new
            {
                iframe = partnerServer,
                partner = partnerCode,
                token,
                lang = session.LanguageId,
                timeZone = session.TimeZone,
                isMobile = isForMobile,
                home = casinoPageUrl
            };
            return string.Format(Provider.GameLaunchUrl, session.Domain, CommonFunctions.GetUriEndocingFromObject(input));
        }
    }
}