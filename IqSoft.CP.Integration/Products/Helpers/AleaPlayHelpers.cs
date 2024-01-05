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
using System.Net;

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

        //private readonly static List<string> PragmaticPlayRestrictedCountries = new List<string>
        //{
        //    "AF", "AL", "DZ", "AD", "AO", "AW", "AU", "BS", "BE", "BQ", "BW", "BG",
        //     "KW", "CW", "CY", "CZ", "DK", "EE", "ET", "FR", "GI", "GY", "HK", "IN",
        //     "IR","IQ","IL","IT","KP","KW","LA","LV","LT","MT","MX","MM","NA","NL",
        //    "NI","PK","PA","PG","PH","PT","RO","MF","RS","SG","SS","ES","LK","SD",
        //    "SE","CH","SY","TW","TT","GB","","",
        //};
        private readonly static List<string> RestrictedCountries = new List<string>
        {
            "FR", "IT", "UK", "GB","MT","PT", "SE", "ES"
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
                    country = RestrictedCountries.Contains(countryCode.ToUpper()) ? "FI" : countryCode,
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
                RequestMethod = Constants.HttpRequestMethods.Get,
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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {environment}");
            var resultList = new List<Game>();
            var gameString = "results{id name software{id name} " +
                "type status genre jackpot freeSpinsCurrencies ratio rtp volatility minBet maxBet maxExposure maxWinMultiplier lines hitFrequency buyFeature " +
                "releaseDate features assetsLink thumbnailLinks }";
            var input = new GraphQLRequest
            {
                Query = "{ gamesReady(jurisdictionCode: \"CAO\", size:500){page{number size totalPages totalElements} " + gameString + "}}",
            };
            var games = graphQLClient.SendQueryAsync<GamesOutput>(input).Result;
            resultList.AddRange(games.Data.GamesReady.Results);
            var totalPages = games.Data.GamesReady.Page.TotalPages;
            for (int i = 1; i < totalPages; i++)
            {
                input = new GraphQLRequest
                {
                    Query = "{ gamesReady(jurisdictionCode: \"CAO\", page:" + i + " size:500){" + gameString + "}}"
                };
                games = graphQLClient.SendQueryAsync<GamesOutput>(input).Result;
                resultList.AddRange(games.Data.GamesReady.Results);
            }

            return resultList;
        }
    }
}