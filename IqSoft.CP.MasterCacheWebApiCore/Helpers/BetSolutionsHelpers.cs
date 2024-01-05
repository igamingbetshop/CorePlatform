using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class BetSolutionsHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.BetSolutions);
        public static string GetUrl(string token, int partnerId, int productId, bool isForDemo, bool isForMobile, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            var merchantId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BetSolutionsMerchantId);
            var gamesDetails = product.ExternalId.Split(',');
            if (gamesDetails.Length != 2)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongProductId);
            var input = new
            {
                Token = token,
                MerchantId = merchantId,
                Lang = session.LanguageId,
                GameId = gamesDetails[1],
                ProductId = gamesDetails[0],
                IsFreeplay = isForDemo ? 1 : 0,
                Platform = isForMobile ? "mobile" : "desktop"
            };
            return string.Format("{0}?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(input));
        }
    }
}