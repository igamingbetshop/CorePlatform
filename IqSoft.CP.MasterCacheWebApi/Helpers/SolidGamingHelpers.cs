using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public class SolidGamingHelpers
    {
        public readonly static Dictionary<string, string> LanguageCodes = new Dictionary<string, string>
        {
            { "zh", "CHI"}
        };

        public static string GetUrl(int partnerId, int productId, string token, int clientId, bool isForDemo, SessionIdentity session)
        {
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.SolidGaming);
            int providerId = gameProvider.Id;
            var product = CacheManager.GetProductById(productId);
            var client = CacheManager.GetClientById(clientId);
            var brandCode = CacheManager.GetGameProviderValueByKey(partnerId, providerId, Constants.PartnerKeys.SolidGamingBrandCode);
            var url = gameProvider.GameLaunchUrl;
            var lobbyUrl = Uri.EscapeDataString(session.Domain);
            var language = LanguageCodes.ContainsKey(session.LanguageId) ? LanguageCodes[session.LanguageId] : 
            ( Integration.CommonHelpers.LanguageISO3Codes.ContainsKey(session.LanguageId) ? Integration.CommonHelpers.LanguageISO3Codes[session.LanguageId] :
                Integration.CommonHelpers.LanguageISO3Codes[Constants.DefaultLanguageId]);
            var mode = isForDemo ? "FUN" : "REAL";
            return string.Format(url, brandCode, product.ExternalId, token, language, mode, lobbyUrl, client.CurrencyId); ;
        }
    }
}