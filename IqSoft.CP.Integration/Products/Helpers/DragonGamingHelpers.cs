using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.DragonGaming;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class DragonGamingHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.DragonGaming);
        public static List<GameProperty> GetGames(int partnerId)
        {
            var gamesUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.DragonApiUrl);
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.DragonApiKey);
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = string.Format("{0}games/get-games/", gamesUrl),
                PostData = JsonConvert.SerializeObject(new { api_key = apiKey })
            };
            var result = JsonConvert.DeserializeObject<GamesOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            var gamesList = new List<GameProperty>();
            gamesList.AddRange(result.Categories.Slots.Select(x => new
            {
                Val = JObject.Parse(JsonConvert.SerializeObject(x)).Properties()
                                                                .Select(y => y.First().ToObject<GameProperty>())
            }).SelectMany(x => x.Val).ToList());
            gamesList.AddRange(result.Categories.TableGames.Select(x => new
            {
                Val = JObject.Parse(JsonConvert.SerializeObject(x)).Properties()
                                                               .Select(y => y.First().ToObject<GameProperty>())
            }).SelectMany(x => x.Val).ToList());

            gamesList.AddRange(result.Categories.ScratchCards.Select(x => new
            {
                Val = JObject.Parse(JsonConvert.SerializeObject(x)).Properties()
                                                               .Select(y => y.First().ToObject<GameProperty>())
            }).SelectMany(x => x.Val).ToList());
            return gamesList;
        }

        public static string GetGameLaunchUrl(int clientId, string token, int partnerId, int productId, bool isForMobile, bool isForDemo, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            var gamesUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.DragonApiUrl);
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.DragonApiKey);
            var partner = CacheManager.GetPartnerById(partnerId);
            var client = CacheManager.GetClientById(clientId);
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
            var externalData = product.ExternalId.Split(',');
            var input = new
            {
                api_key = apiKey,
                session_id = isForDemo ? CommonFunctions.GetRandomString(20) : token,
                provider = Constants.GameProviders.DragonGaming.ToLower(),
                game_type = externalData[1],
                game_id = externalData[0],
                platform = isForMobile ? "mobile" : "desktop",
                language = session.LanguageId,
                amount_type = isForDemo ? "fun" : "real",
                lobby_url = casinoPageUrl,
                deposit_url = cashierPageUrl,
                context = new
                {
                    id = isForDemo ? 0 : clientId,
                    username = isForDemo ? "fun_player" : client.UserName,
                    country = session.Country,
                    currency = isForDemo ? partner.CurrencyId : client.CurrencyId,
                }
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = string.Format("{0}games/game-launch/", gamesUrl),
                PostData = JsonConvert.SerializeObject(input)
            };
            return JsonConvert.DeserializeObject<LaunchResultOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).LaunchResult.Url;
        }
    }
}
