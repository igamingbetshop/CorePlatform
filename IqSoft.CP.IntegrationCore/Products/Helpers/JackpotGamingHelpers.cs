using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.CacheModels;
using System.Collections.Generic;
using IqSoft.CP.Integration.Products.Models.JackpotGaming;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using System.Net.Http;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class JackpotGamingHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.JackpotGaming);

        public static string GetUrl(string token, int clientId, int productId, bool isForDemo, SessionIdentity session, log4net.ILog log)
        {
            if (isForDemo)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DemoNotSupported);
            var product = CacheManager.GetProductById(productId);
            var client = CacheManager.GetClientById(clientId);
            var apiToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.JackpotGamingApiToken);
            var inputData = new
            {
                tokenClient = apiToken,
                userId = client.Id.ToString(),
                gameId = product.ExternalId,
                currency = client.CurrencyId,
                language = session.LanguageId.ToUpper(),
                gateSessionToken = token
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = HttpMethod.Post,
                Url = $"{Provider.GameLaunchUrl}/v2/product/jackpot/launch",
                PostData = JsonConvert.SerializeObject(inputData)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            return JsonConvert.DeserializeObject<LaunchOutput>(response).LaunchUrl;
        }

        public static List<GameItem> GetGames(int partnerId)
        {
            var apiToken = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.JackpotGamingApiToken);
            var httpRequestInput = new HttpRequestInput
            {
                RequestHeaders = new Dictionary<string, string> { { "tokenClient", apiToken} },
                RequestMethod = HttpMethod.Get,
                Url = $"{Provider.GameLaunchUrl}/products/jackpot/games"
            };
            var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);

            return JsonConvert.DeserializeObject<GameList>(res).Games;
        }
    }
}
