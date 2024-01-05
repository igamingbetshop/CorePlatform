using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class HabaneroHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.Habanero);

        public static string GetUrl(string token, int partnerId, int productId, bool isForDemo, SessionIdentity session)
        {
            var brandid = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.HabaneroBrandId);
            var product = CacheManager.GetProductById(productId);
            var inputData = new
            {
                brandid,
                keyname = product.ExternalId,
                token,
                mode = isForDemo ? "fun" : "real",
                locale = session.LanguageId,
                lobbyurl = string.Format("https://{0}", session.Domain)
            };
            return string.Format("{0}/go.ashx?{1}", string.Format(Provider.GameLaunchUrl, "app"), CommonFunctions.GetUriEndocingFromObject(inputData));
        }
    }
}