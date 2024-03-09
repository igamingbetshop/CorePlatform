using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class AleaPartnersHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.AleaPartners);
        public static string GetUrl(int userId, string token, int partnerId, int productId, string languageId)
        {
            var product = CacheManager.GetProductById(productId);
            var partnerName = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.AleaPartnersName);
            var instance = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.AleaPartnersInstance);
            string launchParameters = CommonFunctions.GetUriEndocingFromObject(new
                {
                    useruuid = userId,
                    sessionId = token,
                    partner = partnerName,
                    instance,
                    lang = languageId,
                    game = product.ExternalId == "lobby" ? string.Empty : product.ExternalId
                });

            return $"{Provider.GameLaunchUrl}?{launchParameters}";
        }
    }
}
