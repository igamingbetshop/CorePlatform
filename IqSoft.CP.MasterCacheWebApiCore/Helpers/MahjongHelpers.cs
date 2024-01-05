using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public class MahjongHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.Mahjong);

        public static string GetUrl(string token, int partnerId, int productId, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            var mahjongPartnerId = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.MahjongPartnerId);
            var input = new
            {
                partnerId = mahjongPartnerId?.StringValue,
                sessionKey = token,
                locale = session.LanguageId,
                action = product.ExternalId
            }; 
            return string.Format("{0}?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(input));
        }
    }
}