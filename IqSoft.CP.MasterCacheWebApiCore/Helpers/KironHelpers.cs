using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class KironHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.Kiron);

        public static string GetUrl(string token, int partnerId, int clientId, int productId, bool isForDemo, bool isForMobile, bool isForShop, SessionIdentity session)
        {
            var client = CacheManager.GetClientById(clientId);
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id,
                isForShop ? Constants.PartnerKeys.KironBetShopOperatorId : Constants.PartnerKeys.KironOperatorId);
            var inputData = new
            {
                o = operatorId,
                p = token,
                c = client.CurrencyId,
                l = session.LanguageId,
                dp = isForDemo ? 1 : 0,
                pd = isForMobile ? 2 : 1
            };
            return string.Format("{0}/?{1}", string.Format(Provider.GameLaunchUrl, isForDemo ? "demo" : "games"), CommonFunctions.GetUriEndocingFromObject(inputData));
        }
    }
}