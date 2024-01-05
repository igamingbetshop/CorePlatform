using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public class SkyWindHelpers
    {
        public  static string GetUrl(int partnerId, int productId, int clientId, string token, string languageId)
        {
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.SkyWind);
            var product = CacheManager.GetProductById(productId);
			var client = CacheManager.GetClientById(clientId);
			
			var header = Integration.Products.Helpers.SkyWindHelpers.UserLogin(partnerId);
			if(header == string.Empty)
				throw BaseBll.CreateException(languageId, Constants.Errors.WrongLoginParameters);

			var url = Integration.Products.Helpers.SkyWindHelpers.GetGameLaunchUrl(product.ExternalId, client.Id, token, languageId, header);
			if (url == string.Empty)
				throw BaseBll.CreateException(languageId, Constants.Errors.WrongToken);

			var merchantCode = CacheManager.GetGameProviderValueByKey(partnerId, gameProvider.Id, Constants.PartnerKeys.SkyWindMerchantId);
			return url;
        }
    }
}