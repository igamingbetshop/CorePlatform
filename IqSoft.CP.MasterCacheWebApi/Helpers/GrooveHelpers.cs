using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration;
using System.Collections.Generic;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class GrooveHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.GrooveGaming);
        private static readonly List<string> NotSupportedCurrencies = new List<string>
        {
            Constants.Currencies.USDT
        };
        public static string GetUrl(int clientId, string token, int partnerId, int productId, bool isForDemo, bool isForMobile, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            var partner = CacheManager.GetPartnerById(partnerId);
            var client = CacheManager.GetClientById(clientId);
            if (!isForDemo && client == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
            var casinoId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.GrooveCasinoId);
            var license = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.GrooveLicense);
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);
            var currency = (isForDemo || client == null) ? partner.CurrencyId : client.CurrencyId;
            if (NotSupportedCurrencies.Contains(currency))
                currency = Constants.Currencies.USADollar;
            var input = new
            {
                nogsgameid = product.ExternalId,
                nogsoperatorid = casinoId,
                sessionid = string.Format("{0}_{1}", casinoId, token),
                nogscurrency = currency,
                nogslang = CommonHelpers.LanguageISOCodes[session.LanguageId].ToLower(),
                nogsmode = !isForDemo ? "real" : "demo",
                accountid = !isForDemo ? client.Id.ToString() : "DummyId",
                homeurl = casinoPageUrl,
                device_type = isForMobile ? "mobile" : "desktop",
                country = session.Country,
                is_test_account = false,
                license
            };
            return string.Format("{0}?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(input));
        }
    }
}