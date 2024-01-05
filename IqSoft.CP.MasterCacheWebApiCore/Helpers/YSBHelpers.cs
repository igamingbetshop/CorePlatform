using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public class YSBHelpers
    {
        public static string GetUrl(int partnerId, int productId, string token, int clientId, string languageId, bool isForMobile, bool isForDemo)
        {

            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.YSB);
            int providerId = gameProvider.Id;
            var partner = CacheManager.GetPartnerById(partnerId);
            var baseUrl = CacheManager.GetGameProviderValueByKey(partnerId, providerId,
               Constants.PartnerKeys.YSBBaseUrl + (isForMobile ? "Mobile" : "Desktop") + partner.CurrencyId);

            var url = gameProvider.GameLaunchUrl;
            if (isForDemo)
                return baseUrl + "?visit=1";
            var product = CacheManager.GetProductById(productId);

            var client = CacheManager.GetClientById(clientId);
            var prefix = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.YSBPrefix + client.CurrencyId);
            var vendor = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.YSBVendor + client.CurrencyId);

           
            var curr = client.CurrencyId == "CNY" ? "RMB" : client.CurrencyId;
            return string.Format(url, baseUrl, prefix + clientId, languageId, token, vendor, curr);
        }
    }
}