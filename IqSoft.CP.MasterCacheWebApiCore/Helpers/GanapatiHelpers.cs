using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class GanapatiHelpers
    {
        public static string GetUrl(int partnerId, int productId, string token, int clientId, bool isForDemo, SessionIdentity session)
        {
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.Ganapati);
            var product = CacheManager.GetProductById(productId);
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, gameProvider.Id, Constants.PartnerKeys.GanapatiOperatorId);
            var partner = CacheManager.GetPartnerById(partnerId);
            var url = string.Format(gameProvider.GameLaunchUrl, operatorId);
            if (isForDemo)
            {
                var demoInput = new
                {
                    game = product.ExternalId,
                    locale = session.LanguageId.ToLower(),
                    mode = "fun"
                };
                url += CommonFunctions.GetUriEndocingFromObject(demoInput);
            }
            else
            {
                var client = CacheManager.GetClientById(clientId);

                var input = new
                {
                    game = product.ExternalId,
                    @operator = operatorId,
                    currency = client.CurrencyId,
                    locale = session.LanguageId.ToLower(),
                    mode = "real",
                    launchToken = token,
                    lobbyURL = string.Format("https://{0}", session.Domain),
                    brandId = partner.Name
                };
                url += CommonFunctions.GetUriEndocingFromObject(input);
            }
            return url;
        }
    }
}