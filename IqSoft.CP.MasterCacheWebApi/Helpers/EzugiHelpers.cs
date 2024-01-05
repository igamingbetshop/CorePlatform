using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using static IqSoft.CP.Common.Constants;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class EzugiHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(GameProviders.Ezugi);

        public static string GetUrl(int partnerId, string token, bool isForDemo, BllProduct product, SessionIdentity session)
        {
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EzugiOperatorId);
            if (product.SubProviderId.HasValue)
            {
                var subProvider = CacheManager.GetGameProviderById(product.SubProviderId.Value);
                if (subProvider.Name == Constants.GameProviders.Evolution || subProvider.Name == Constants.GameProviders.NetEnt || subProvider.Name.Contains(Constants.GameProviders.RedTiger))
                    return string.Format("{0}?token={1}&operatorId={2}&clientType=html5&language={3}&homeUrl=https://{4}&openTable={5}", Provider.GameLaunchUrl,
                                            isForDemo ? "demo" : token, operatorId, session.LanguageId, session.Domain, product.ExternalId);
            }
            var liveCasinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.LiveCasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(liveCasinoPageUrl))
                liveCasinoPageUrl = string.Format("https://{0}/products/5", session.Domain);
            else
                liveCasinoPageUrl = string.Format(liveCasinoPageUrl, session.Domain);

            return string.Format("{0}?token={1}&operatorId={2}&clientType=html5&language={3}&homeUrl={4}", Provider.GameLaunchUrl,
                        isForDemo ? "demo" : token, operatorId, session.LanguageId, liveCasinoPageUrl);
        }

		public static string GetTablesInfoFromProvider(int partnerId, int providerId, string currencyId)
        {
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EzugiOperatorId);
            CacheManager.GetLiveGamesLobbyItems(providerId, operatorId, currencyId);
            return Integration.Products.Helpers.EzugiHelpers.GetMessagesFromProviderServer(operatorId, currencyId);
        }
    }
}