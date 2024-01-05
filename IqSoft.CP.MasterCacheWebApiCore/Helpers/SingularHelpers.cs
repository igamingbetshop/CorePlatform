using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class SingularHelpers
    {
        public static string GetUrl(string partnerName,  string token, bool isForMobile, SessionIdentity session)
        {
            var gameLaunchUrl = CacheManager.GetGameProviderByName(Constants.GameProviders.Singular).GameLaunchUrl;
            if (!isForMobile)
            {
                gameLaunchUrl = string.Format(gameLaunchUrl, 
                    partnerName.ToLower(), "", string.Format("key={0}&lang={1}", token, session.LanguageId));
            }
            else
            {
                gameLaunchUrl = string.Format(gameLaunchUrl,
                    partnerName.ToLower(), "mobile/", string.Format("key={0}&lang={1}&exit_url={2}", token, session.LanguageId, "http://" + session.Domain));
            }
            return gameLaunchUrl;
        }
    }
}