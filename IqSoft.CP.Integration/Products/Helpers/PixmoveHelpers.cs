using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Products.Models.Pixmove;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class PixmoveHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.Pixmove);
        public static List<GameItem> GetGames(int partnerId, SessionIdentity session, ILog log)
        {
            var partner = CacheManager.GetPartnerById(partnerId);
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PixmoveApiKey);
            var operatorIdKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PixmovePartnerId);
            if (!int.TryParse(operatorIdKey, out int operatorId))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerKeyNotFound);
            var requestInput = new
            {
                partnerId = operatorId,
                hash = CommonFunctions.ComputeHMACSha256($"game_list{operatorId}{apiKey}", apiKey).ToLower()
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = $"{Provider.GameLaunchUrl}/game/list?{CommonFunctions.GetUriDataFromObject(requestInput)}"
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var gameOutput = JsonConvert.DeserializeObject<GameOutput>(response);
            if (gameOutput.Status?.ToLower() != "success")
                throw new Exception(response);
            return gameOutput.Data.Games;
        }

        public static string GetLaunchUrl(int partnerId, string token, int clientId, int productId, bool isForDemo, SessionIdentity session)
        {
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PixmoveApiKey);
            var operatorIdKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.PixmovePartnerId);
            if (!int.TryParse(operatorIdKey, out int operatorId))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerKeyNotFound);
            var product = CacheManager.GetProductById(productId);
            var casinoPageUrl = PartnerBll.GetCasinoPageUrl(partnerId, session.Domain);
            var type = "demo";
            BllClient client = null;
            if (!isForDemo)
            {
                client = CacheManager.GetClientById(clientId) ??
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                type = "game";
            }
          
            var requestInput = new
            {
                currency = client?.CurrencyId,
                partnerId = operatorId,
                gameId = Convert.ToInt32(product.ExternalId),
                playerId = client?.Id.ToString(),
                sessionId = token,
                language = session.LanguageId,
                returnUrl = casinoPageUrl,
                hash = CommonFunctions.ComputeHMACSha256($"{type}{client?.CurrencyId}{operatorId}{product.ExternalId}{client?.Id}{token}" +
                                                         $"{session.LanguageId}{casinoPageUrl}{apiKey}",apiKey).ToLower()
            };
            if (!isForDemo)
                type = "init";
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = $"{Provider.GameLaunchUrl}/game/{type}?{CommonFunctions.GetUriDataFromObject(requestInput)}"
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var gameLaunchOutput = JsonConvert.DeserializeObject<GameLaunchOutput>(response);
            if (gameLaunchOutput.Status?.ToLower() != "success")
                throw new Exception(response);
            return gameLaunchOutput.Data.GameUrl;
        }
    }
}
