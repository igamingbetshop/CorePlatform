using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Helpers;

namespace IqSoft.CP.BetShopGatewayWebApi.Helpers
{
    public static class ProductHelpers
    {
        public static string GetProductLaunchUrl(int productId, string token, SessionIdentity cashierSession)
        {
            var product = CacheManager.GetProductById(productId)??
                throw BaseBll.CreateException(cashierSession.LanguageId, Constants.Errors.ProductNotFound);
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(cashierSession.PartnerId, productId) ??
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerProductSettingNotFound);
            if (partnerProductSetting.State == (int)PartnerProductSettingStates.Blocked)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.ProductNotAllowedForThisPartner);

            var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
            switch(provider.Name)
            {
                case Constants.GameProviders.AleaPartners:
                    return AleaPartnersHelpers.GetUrl(cashierSession.Id, token, cashierSession.PartnerId, productId, cashierSession.LanguageId);
                case Constants.GameProviders.Kiron:
                    return KironHelpers.GetUrl(token, cashierSession.PartnerId, cashierSession.CurrencyId, false, false, true, cashierSession);
                case Constants.GameProviders.Internal:
                case Constants.GameProviders.IqSoft:
                    return string.Empty;
                default:
                    throw BaseBll.CreateException(cashierSession.LanguageId, Constants.Errors.WrongProviderId);
            }
        }
    }
}