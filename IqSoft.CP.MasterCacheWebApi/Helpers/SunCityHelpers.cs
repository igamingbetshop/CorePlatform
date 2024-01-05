using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public class SunCityHelpers
    {
        public static string GetUrl(int partnerId, int productId,  int clientId, bool isForMobile, SessionIdentity session)
        {
            var token = Integration.Products.Helpers.SunCityHelpers.GeneratePlayerToken(clientId, session, isForMobile);

            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.SunCity);
            int providerId = gameProvider.Id;

            var url = gameProvider.GameLaunchUrl;
            var product = CacheManager.GetProductById(productId);
            var client = CacheManager.GetClientById(clientId);
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.SunCityOperatorID + client.CurrencyId);

            return string.Format(url, operatorId, token);
        }
    }
}