using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class TurboGamesHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.TurboGames);
        public static string GetUrl( string token, int partnerId, int productId, bool isForDemo, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            var cid = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TurboGamesClientId);

            var input = new
            {
                locale= session.LanguageId,
                cid,
                token,
                demo = isForDemo
            };
            return string.Format("{0}?{1}", string.Format(Provider.GameLaunchUrl, product.ExternalId), CommonFunctions.GetUriEndocingFromObject(input));
        }
    }
}