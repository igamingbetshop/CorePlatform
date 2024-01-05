using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.Mancala;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class MancalaHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.Mancala);
        public static string GetGameLaunchUrl(int partnerId, int clientId, string token, int productId, bool isForDemo, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            var casinoPartnerId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.MancalaCasinoId);
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.MancalaApiKey);
            string jsonInput;
            if (isForDemo)
            {
                var demoHash = CommonFunctions.ComputeMd5(string.Format("GetToken/{0}{1}{2}", casinoPartnerId, product.ExternalId, apiKey));
                var demoInput = new
                {
                    ClientGuid = casinoPartnerId,
                    GameId = Convert.ToInt32(product.ExternalId),
                    Lang = session.LanguageId,
                    IsVirtual = false,
                    Hash = demoHash,
                    DemoMode = isForDemo
                };
                jsonInput = JsonConvert.SerializeObject(demoInput);
            }
            else
            {
                var client = CacheManager.GetClientById(clientId);
                var hash = string.Format("GetToken/{0}{1}{2}{3}{4}", casinoPartnerId, product.ExternalId,
                                                                     client.Id, client.CurrencyId, apiKey);
                hash = CommonFunctions.ComputeMd5(hash);
                var requestInput = new
                {
                    ClientGuid = casinoPartnerId,
                    GameId = Convert.ToInt32(product.ExternalId),
                    UserId = clientId.ToString(),
                    Currency = client.CurrencyId,
                    Lang = CommonHelpers.LanguageISOCodes[session.LanguageId],
                    IsVirtual = false,
                    Hash = hash,
                    DemoMode = isForDemo,
                    ExtraData = token
                };
                jsonInput = JsonConvert.SerializeObject(requestInput);
            }
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url =  Provider.GameLaunchUrl + "/GetToken",
                PostData = jsonInput
            };
            var resp = JsonConvert.DeserializeObject<GetTokenOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (resp.Error != 0)
                throw new Exception(string.Format("Code: {0}, Description: {1}", resp.Error, resp.Msg));
            return resp.IframeUrl;
        }

        public static List<GameItem> GetGames(int partnerId)
        {
            var casinoPartnerId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.MancalaCasinoId);
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.MancalaApiKey);
            var requestInput = new
            {
                ClientGuid = casinoPartnerId,
                Hash = CommonFunctions.ComputeMd5(string.Format("GetAvailableGames/{0}{1}", casinoPartnerId, apiKey))
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url =  Provider.GameLaunchUrl + "/GetAvailableGames",
                PostData = JsonConvert.SerializeObject(requestInput)
            };
            var resp = JsonConvert.DeserializeObject<GamesOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (resp.Error != 0)
                throw new Exception(string.Format("Code: {0}, Description: {1}", resp.Error, resp.Msg));
            return resp.Games;
        }
    }
}