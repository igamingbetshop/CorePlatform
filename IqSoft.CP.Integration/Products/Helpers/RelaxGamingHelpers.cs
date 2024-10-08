using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.Bonus;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.RelaxGaming;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class RelaxGamingHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.RelaxGaming);
        public static string GetUrl(int clientId, string token, int partnerId, int productId, bool isForDemo, bool isMobile, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.RelaxGamingPartnerId);
            var operatorName = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.RelaxGamingPartnerName);
            var partner = CacheManager.GetPartnerById(partnerId);
            var currency = partner.CurrencyId;
            if (clientId > 0)
            {
                var client = CacheManager.GetClientById(clientId);
                currency = client.CurrencyId;
            }

            var input = new
            {
                gameid = product.ExternalId,
                ticket = token,
                jurisdiction = session.Country,
                lang = CommonHelpers.LanguageISOCodes.ContainsKey(session.LanguageId) ? CommonHelpers.LanguageISOCodes[session.LanguageId] :
                                                                                        CommonHelpers.LanguageISOCodes[Constants.DefaultLanguageId],
                channel = !isMobile ? "web" : "mobile",
                partnerid = operatorId,
                partner = operatorName,
                moneymode = isForDemo ? "fun" : "real",
                currency,
                homeurl = PartnerBll.GetCasinoPageUrl(partnerId, session.Domain),

            };
            return $"{Provider.GameLaunchUrl}?{CommonFunctions.GetUriEndocingFromObject(input)}";
        }

        public static List<GameItem> GetGames(int partnerId)
        {
            //Get Providers
            //Get Lobbies
            //Get Available Games
            var gamesList = new List<GameItem>();
            var apiUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.RelaxGamingApiUrl);
            var apiUsername = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.RelaxGamingApiUsername);
            var apiPassword = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.RelaxGamingApiPassword);
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{apiUsername}:{apiPassword}")) } },
                Url = $"{apiUrl}/providers/get"
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var providers = JsonConvert.DeserializeObject<List<ProviderItem>>(response);
            if (!providers.Any())
                throw new Exception(response);
            foreach (var provider in providers)
            {
                try
                {
                    httpRequestInput.Url=$"{apiUrl}/lobbies/get/?provider={provider.Provider}";
                    response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    var lobbiesOutput = JsonConvert.DeserializeObject<LobbiesOutput>(response);
                    if (lobbiesOutput.Status?.ToLower() != "error")
                        gamesList.AddRange(lobbiesOutput.Lobbies.Select(x => new GameItem { GameId = x.LobbyId, Name = x.Name, Channels = new List<string> { "web", "mobile" } }));
                }
                catch
                {; }
            }
            httpRequestInput.Url=$"{apiUrl}/games/getgames";
            response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var gamesOutput = JsonConvert.DeserializeObject<GamesOutput>(response);
            if (gamesOutput.Status.ToLower() != "ok")
                throw new Exception(response);
            gamesList.AddRange(gamesOutput.Games);
            return gamesList;
        }

        public static bool AddFreeRound(FreeSpinModel freeSpinModel, ILog log)
        {
            var client = CacheManager.GetClientById(freeSpinModel.ClientId);
            var apiUrl = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.RelaxGamingApiUrl);
            var apiUsername = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.RelaxGamingApiUsername);
            var apiPassword = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.RelaxGamingApiPassword);
            var product = CacheManager.GetProductByExternalId(Provider.Id, freeSpinModel.ProductExternalId);
            decimal? bValue = null;
            if (!string.IsNullOrEmpty(freeSpinModel.BetValues))
            {
                var bv = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(freeSpinModel.BetValues);
                if (bv.ContainsKey(client.CurrencyId))
                    bValue = bv[client.CurrencyId];
            }
            if (bValue == null && !string.IsNullOrEmpty(product.BetValues))
            {
                var bv = JsonConvert.DeserializeObject<Dictionary<string, List<decimal>>>(product.BetValues);
                if (bv.ContainsKey(client.CurrencyId) && bv[client.CurrencyId].Any())
                    bValue = bv[client.CurrencyId][0];
            }
            if (bValue == null)
                return false;
            var input = new
            {
                txid ="RelaxGaming_" + freeSpinModel.BonusId,
                remoteusername = client.Id.ToString(),
                gameid = freeSpinModel.ProductExternalId,
                amount = freeSpinModel.SpinCount,
                freespinvalue = bValue.Value * 100,
                expire = ((DateTimeOffset)freeSpinModel.FinishTime).ToUnixTimeSeconds(),
                currency = client.CurrencyId,
                playercurrency = client.CurrencyId
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = $"{apiUrl}/freespins/add",
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{apiUsername}:{apiPassword}")) } },
                PostData = JsonConvert.SerializeObject(input)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            log.Error($"Input: {JsonConvert.SerializeObject(httpRequestInput)},  Output: {response}");
            var res = JsonConvert.DeserializeObject<FreeRoundOutput>(response);
            if (res.Status.ToLower() != "ok")
            {
                log.Error($"Input: {JsonConvert.SerializeObject(input)},  Output: {res}");
                return false;
            }
            return true;
        }

    }
}