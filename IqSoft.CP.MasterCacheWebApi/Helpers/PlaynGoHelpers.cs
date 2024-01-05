using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class PlaynGoHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.PlaynGo);
        public static string GetUrl(string token, int partnerId, int productId, bool isForDemo, bool IsForMobile, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PlaynGoApiPId);
         
            var input = new
            {
                gid = product.ExternalId.Split('-')[1],
                lang = CommonHelpers.LanguageISOCodes[session.LanguageId],
                pid = operatorId,
                practice = isForDemo ? 1 : 0,
                ticket = token,
                channel = IsForMobile ? "mobile" : "desktop",
                origin = session.Domain
            };
            return string.Format("{0}/casino/ContainerLauncher?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(input));
        }
    }
}