using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using System.Collections.Generic;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class ISoftBetHelpers
    {
        public readonly static Dictionary<string, string> LanguageCodes = new Dictionary<string, string>
        {
            { "zh", "CHS"}
        };

        public static string GetUrl(int productId, string token, int clientId, bool isForDemo, SessionIdentity session)
        {
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.ISoftBet);
            var product = CacheManager.GetProductById(productId);
            var client = CacheManager.GetClientById(clientId);
            var url = gameProvider.GameLaunchUrl;
            var mode = isForDemo ? 0 : 1;
            var language = LanguageCodes.ContainsKey(session.LanguageId) ? LanguageCodes[session.LanguageId] : session.LanguageId;

            return string.Format(url, product.ExternalId, language, client.CurrencyId, mode,
                client.PartnerId + "_" + client.UserName, clientId, token, session.Domain);
        }
    }
}