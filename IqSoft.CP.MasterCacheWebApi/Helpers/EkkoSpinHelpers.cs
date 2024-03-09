using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class EkkoSpinHelpers
    {
        internal static string GetUrl(BllPartner partner, int clientId, int productId, string urlBase, string lang)
        {
            string externalId = CacheManager.GetProductById(productId).ExternalId;
            decimal balance = (long)(BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientId).AvailableBalance * 100) / 100m;
            string secretKey = GetEkkoSpinSecretKey(partner.Id);
            return Integration.Products.Helpers.EkkoSpinHelpers.GetUrlFromProvider(clientId, urlBase, balance, externalId, lang, secretKey);
        }

        private static string GetEkkoSpinSecretKey(int partnerId)
        {
            var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, CacheManager.GetGameProviderByName(Constants.GameProviders.EkkoSpin).Id, Constants.PartnerKeys.EkkoSpinSecretKey);

            if (partnerKey == null || partnerKey == string.Empty)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerKeyNotFound);
            return partnerKey;
        }
    }
}