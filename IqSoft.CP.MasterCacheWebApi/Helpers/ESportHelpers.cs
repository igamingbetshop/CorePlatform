using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public class ESportHelpers
    {
        public static string GetUrl(int partnerId, int productId, string token, int clientId)
        {
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.ESport);
            var url = gameProvider.GameLaunchUrl;
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, gameProvider.Id, Constants.PartnerKeys.ESportOperatorId);
            return string.Format(url, operatorId, token, clientId);
        }
    }
}