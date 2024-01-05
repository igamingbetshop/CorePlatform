using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class EvoplayHelpers
    {
        private static readonly BllGameProvider GameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.Evoplay);
        private static readonly string ParametersTemplate = @"project={0}&version={1}&token={2}&game={3}&" +
                                            "settings[user_id]={4}&settings[exit_url]={5}&settings[cash_url]={6}&settings[language]={7}&settings[https]={8}&" +
                                            "denomination={9}&currency={10}&return_url_info={11}&callback_version={12}";

        public static string GetUrl(int partnerId, int clientId, string token, int productId, bool isForDemo, SessionIdentity session)
        {
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, GameProvider.Id, Constants.PartnerKeys.EvoplayApiKey);
            var projectId = CacheManager.GetGameProviderValueByKey(partnerId, GameProvider.Id, Constants.PartnerKeys.EvoplayProjectId);
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

            var cashierPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashierPageUrl).StringValue;
            if (string.IsNullOrEmpty(cashierPageUrl))
                cashierPageUrl = string.Format("https://{0}/user/1/deposit", session.Domain);
            else
                cashierPageUrl = string.Format(cashierPageUrl, session.Domain);

            var product = CacheManager.GetProductById(productId);
            string parameters;
            string signString;
            if (isForDemo)
            {
                var partner = CacheManager.GetPartnerById(partnerId);
                var demoInp= new
                {
                    project = projectId,
                    version = 1,
                    token = "demo",
                    game = product.ExternalId,
                    settings = new
                    {
                        user_id = "test",
                        exit_url = casinoPageUrl,
                        cash_url = cashierPageUrl,
                        language = session.LanguageId,
                        https = 1                        
                    },
                    denomination = 1,
                    currency = partner.CurrencyId,
                    return_url_info = 1,
                    callback_version = 1
                };
                parameters = string.Format(ParametersTemplate, projectId, 2, demoInp.token, demoInp.game, demoInp.settings.user_id, demoInp.settings.exit_url,
                                           demoInp.settings.cash_url,demoInp.settings.language, demoInp.settings.https, demoInp.denomination,
                                           demoInp.currency, demoInp.return_url_info, demoInp.callback_version);
                signString = Integration.Products.Helpers.EvoplayHelpers.GetSignatureString(demoInp);
            }
            else
            {
                var client = CacheManager.GetClientById(clientId);

                var inp = new
                {
                    project = projectId,
                    version = 1,
                    token,
                    game = product.ExternalId,
                    settings = new
                    {
                        user_id = clientId,
                        exit_url = casinoPageUrl,
                        cash_url = cashierPageUrl,
                        language = session.LanguageId,
                        https = 1                      
                    },
                    denomination = 1,
                    currency = client.CurrencyId,
                    return_url_info = 0,
                    callback_version = 1
                };
                parameters = string.Format(ParametersTemplate, projectId, 1, inp.token, inp.game, inp.settings.user_id, inp.settings.exit_url, inp.settings.cash_url,
                    inp.settings.language, inp.settings.https, inp.denomination, inp.currency, inp.return_url_info, inp.callback_version);

                signString = Integration.Products.Helpers.EvoplayHelpers.GetSignatureString(inp);
            }
            return string.Format("{0}/Game/getURL?signature={1}&{2}", GameProvider.GameLaunchUrl, CommonFunctions.ComputeMd5(signString + "*"+ apiKey), parameters);

        }

    }
}