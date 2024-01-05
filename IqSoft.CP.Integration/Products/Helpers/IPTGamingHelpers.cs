using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using IqSoft.CP.Integration.Products.Models.IPTGaming;
using IqSoft.CP.BLL.Services;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class IPTGamingHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.IPTGaming);

        public static string GetUrl(int clientId, string token, int productId, SessionIdentity session)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client==null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
            var product = CacheManager.GetProductById(productId);
            var casinoId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.ITPGamingPartnerId);
            var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.ITPGamingAPIKey);
            //var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            //if (string.IsNullOrEmpty(casinoPageUrl))
            //    casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            //else
            //    casinoPageUrl = string.Format(casinoPageUrl, session.Domain);
            var input = new
            {
                PartnerId = casinoId,
                GameId = product.ExternalId,
                PlayerId = clientId,
                Currency = client.CurrencyId,
                Lang = session.LanguageId,
                Role = 3, //(player)
                Session = token,
                key = apiKey
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = Provider.GameLaunchUrl,
                PostData = JsonConvert.SerializeObject(input)
            };
            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var launchOutput = JsonConvert.DeserializeObject<LaunchOutput>(resp);
            if (string.IsNullOrEmpty(launchOutput.Url))
                throw new System.Exception(resp);
            return launchOutput.Url;
        }
    }
}