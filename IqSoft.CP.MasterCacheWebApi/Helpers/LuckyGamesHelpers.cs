using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Cache;
namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class LuckyGamesHelpers
    {
        private readonly static BllGameProvider GameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.LuckyGames);
        public static string GetUrl( int productId, string token, string languageId, bool isForDemo)
        {
            var product = CacheManager.GetProductById(productId);
            return string.Format(GameProvider.GameLaunchUrl, product.ExternalId, (isForDemo ? string.Empty : token), languageId);
        }
    }
}