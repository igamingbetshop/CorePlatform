using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class BASHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.BAS);

        public static string GetUrl(int partnerId, int productId, string token, int clientId, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);

            if (product.CategoryId == (int)Constants.ProiductCategories.Lottery)
            {
                var input = new
                { customerId = clientId, token };
                var launchUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BASLotteryLaunchUrl);
                return string.Format("{0}?{1}", launchUrl, CommonFunctions.GetUriEndocingFromObject(input));
            }
            else if (product.CategoryId == (int)Constants.ProiductCategories.LiveGames)
            {
                var input = new
                { Lang = session.LanguageId, token, Brand = clientId };
                var launchUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BASDGSLaunchUrl);
                return string.Format("{0}?{1}", launchUrl, CommonFunctions.GetUriEndocingFromObject(input));
            }
            var inputData = new
            {
                gameId = product.ExternalId,
                token
            };
            return string.Format("{0}?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(inputData));
        }
    }
}