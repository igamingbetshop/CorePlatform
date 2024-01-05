using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class WinSystemsHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.WinSystems);

        public static string GetUrl(int productId, string token)
        {
            var product = CacheManager.GetProductById(productId);
            var inputData = new
            {
                gameId = product.ExternalId,
                token
            };
            return string.Format("{0}?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(inputData));
        }
    }
}