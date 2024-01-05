using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class GoldenRaceHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.GoldenRace);
        public static string GetUrl(string token, int partnerId, int productId, bool isForDemo, bool IsForMobile, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            var privateKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.GoldenRaceApiKey);
            var hostName = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.GoldenRaceHostName);
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

            var mode = isForDemo ? "0" : "1";
            var platform = IsForMobile ? "mobile" : "desktop";
            var input = new
            {
                token,
                game = product.ExternalId,
                backurl = casinoPageUrl,
                mode,
                language = session.LanguageId,
                clientPlatform = platform,
                h = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}{4}{5}{6}", token, product.ExternalId, casinoPageUrl,
                                                                                       mode, session.LanguageId, platform, privateKey))
            };
            var launchUrl = string.Format("{0}?{1}", string.Format(Provider.GameLaunchUrl, hostName), CommonFunctions.GetUriEndocingFromObject(input));
            if (!launchUrl.Contains("token"))
                launchUrl +="&token=";
            return launchUrl;
        }
    }
}