using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using IqSoft.CP.Integration.Products.Models.AleaPlay;
using System.Collections.Generic;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL;
using System.Net.Http;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class AleaPlayHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.AleaPlay);

        public readonly static Dictionary<string, List<string>> NotSupportedCurrencies = new Dictionary<string, List<string>>
        {
            { "playtech", new List<string>{ Constants.Currencies.TunisianDinar } },
            { "amatic", new List<string>{Constants.Currencies.RussianRuble, Constants.Currencies.TunisianDinar } }
        };

        public readonly static Dictionary<string, List<string>> NotSupportedCoutries = new Dictionary<string, List<string>>
        {
            { "playtech", new List<string>{"TN" } },
            { "amatic", new List<string>{"TN","RU", "GB","AM","FR" } },
            { "yggdrasil", new List<string>{"TN" } },
            { "quickspin", new List<string>{"TN" } }
        };

        public readonly static Dictionary<string, string> SupportedCoutries = new Dictionary<string, string>
        {
            { "playtech", "AM"  },
            { "amatic", "BG" },
            { "yggdrasil", "AM" },
            { "quickspin", "AM" }
        };

        public static string GetUrl(string token, int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            var casinoId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.AleaPlayCasinoId);
            var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.AleaPlaySecretKey);
            var environment = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.AleaPlayEnvironment);
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);
            var cashierPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashierPageUrl).StringValue;
            if (string.IsNullOrEmpty(cashierPageUrl))
                cashierPageUrl = string.Format("https://{0}/user/1/deposit/", session.Domain);
            else
                cashierPageUrl = string.Format(cashierPageUrl, session.Domain);
            var launchUrl = string.Empty;
            var signature = string.Empty;
            var subProviderName = string.Empty;
            if (product.SubProviderId.HasValue)
            {
                var subProvider = CacheManager.GetGameProviderById(product.SubProviderId.Value);
                subProviderName = subProvider.Name.ToLower();
            }

            if (!isForDemo)
            {
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                var currency = client.CurrencyId;
                var countryCode = session.Country;
                if (NotSupportedCurrencies.ContainsKey(subProviderName) && NotSupportedCurrencies[subProviderName].Contains(currency))
                    currency = Constants.Currencies.USADollar;
                if (NotSupportedCoutries.ContainsKey(subProviderName) && NotSupportedCoutries[subProviderName].Contains(countryCode))
                    countryCode = SupportedCoutries[subProviderName];

                var input = new
                {
                    casinoId,
                    casinoPlayerId = clientId,
                    casinoSessionId = token,
                    gameId = product.ExternalId,
                    country = countryCode,
                    currency,
                    lobbyUrl = casinoPageUrl,
                    depositUrl = cashierPageUrl,
                    device = session.DeviceType == (int)DeviceTypes.Desktop ? "DESKTOP" : "MOBILE",
                    locale = CommonHelpers.LanguageISOCodes[session.LanguageId]
                };
                launchUrl =  string.Format("{0}/real?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(input));
                signature = CommonFunctions.ComputeSha512($"{input.casinoPlayerId}{input.casinoSessionId}{input.country}{input.currency}{secretKey}").ToLower();
            }
            else
            {
                var partner = CacheManager.GetPartnerById(partnerId);
                var currency = partner.CurrencyId;
                var countryCode = session.Country;
                if (NotSupportedCurrencies.ContainsKey(subProviderName) && NotSupportedCurrencies[subProviderName].Contains(currency))
                    currency = Constants.Currencies.USADollar;
                if (NotSupportedCoutries.ContainsKey(subProviderName) && NotSupportedCoutries[subProviderName].Contains(countryCode))
                    countryCode = SupportedCoutries[subProviderName];
                var demoInput = new
                {
                    casinoId,
                    casinoPlayerId = 0,
                    casinoSessionId = string.Empty,
                    gameId = product.ExternalId,
                    country = countryCode,
                    currency,
                    lobbyUrl = casinoPageUrl,
                    depositUrl = cashierPageUrl,
                    device = session.DeviceType == (int)DeviceTypes.Desktop ? "DESKTOP" : "MOBILE",
                    locale = CommonHelpers.LanguageISOCodes[session.LanguageId]
                };
                launchUrl =  string.Format("{0}/demo?{1}", Provider.GameLaunchUrl, CommonFunctions.GetUriEndocingFromObject(demoInput));
                signature = CommonFunctions.ComputeSha512($"{session.Country}{partner.CurrencyId}{secretKey}");
            }

            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Get,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestHeaders = new Dictionary<string, string> { { "Digest", $"SHA-512={signature}" }, { "Authorization", $"Bearer {environment}" } },
                Url = launchUrl
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var launchOutput = JsonConvert.DeserializeObject<LaunchOutput>(response);
            if (string.IsNullOrEmpty(launchOutput.url))
                throw new System.Exception(response);
            return launchOutput.url;
        }

        public static List<Game> GetGames(int partnerId)
        {
            var environment = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.AleaPlayEnvironment);
            var url = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.AleaPlayGamesUrl);
            var graphQLClient = new GraphQLHttpClient(url, new NewtonsoftJsonSerializer());
            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {environment}");
            var resultList = new List<Game>();
            var activeSoftwareIds = new Dictionary<int, string> {
                {1, "ELK Studios" },
                {42, "Evolution" },
                {32, "NetEnt" },
                {4, "Red Tiger Gaming" },
                {2, "Felix Gaming" },
                {6, "Pragmatic Play" },
                {3, "Spinthon" },
                {74, "Absolute Live Gaming" },
                {75, "Genesis" },
                {77, "Realistic Games" },

                {36, "Booongo" },
                {73, "Ezugi" },
                {63, "Hacksaw Gaming" },
                {35, "Yggdrasil" },
                {9, "Booming Games" },
                {84, "Caleta" },
                {7, "Fugaso" },
                {80, "Habanero" },
                {89, "Microgaming" },
                {16, "Kalamba" },
                {67, "Leap" },
                {65, "Play Pearls" },
                {64, "Ruby Play" },
                {66, "Salsa Technology" },
                {88, "Vivo Gaming" },
                {43, "Nolimit City" },
                {10, "Play'n GO" },
                {5, "Playson" },
                {41, "Playtech" },
                {8, "Quickspin" },
                {29, "Amatic" },
                {23, "PushGaming" },
                {30, "EGT" },
                {21, "BTG" }
            };
            foreach (var softwareId in activeSoftwareIds.Keys)
            {
                var input = new GraphQLRequest
                {
                    Query = "{ gamesReadyToPlay(country: \"AM\", request: { softwareId: " + softwareId + " }) { id name software { id name } " +
                              " categories { id name } configurations { id device attributes { name value description } } } }"
                };
                //var graphQLResponse = graphQLClient.SendQueryAsync<ProvidersOutput>(input).Result;
                //var softwares = string.Empty;
                //graphQLResponse.Data.Software.Results.ForEach(x => softwares += $"softwareId: {x.Id}, ");
                //softwares = softwares.Remove(softwares.Length - 2, 2);

                //input.Query = "{ gamesReadyToPlay(country: \"AM\", request: { softwareId: 6 }) { id name software { id name } " +
                //              " categories { id name } configurations { id device attributes { name value description } } } }";
                var games = graphQLClient.SendQueryAsync<GamesOutput>(input).Result;
                resultList.AddRange(games.Data.GamesReadyToPlay);
            }
            return resultList;
        }
    }
}