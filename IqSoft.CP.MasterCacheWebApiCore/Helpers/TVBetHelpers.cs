using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class TVBetHelpers
    {
        public static string GetUrl(string token, int partnerId, SessionIdentity session)
        {
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.TVBet);
            var tvBetPartnerId = CacheManager.GetGameProviderValueByKey(partnerId, gameProvider.Id, Constants.PartnerKeys.TVBetPartnerId);
            var iframe = CacheManager.GetGameProviderValueByKey(partnerId, gameProvider.Id,Constants.PartnerKeys.TVBetIframe);

            return string.Format(gameProvider.GameLaunchUrl, session.Domain, iframe,  session.LanguageId, token, tvBetPartnerId);
        }
    }
}