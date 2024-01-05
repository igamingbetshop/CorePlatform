using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class RacebookHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.Racebook);

        public static string GetUrl(int clientId, string token, bool isForDemo, SessionIdentity session)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null || isForDemo)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.DemoNotSupported);
            var siteId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.RacebookSiteId);
            var inputData = new
            {
                siteId,
                token
            };
            return string.Format("{0}?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(inputData));
        }
    }
}