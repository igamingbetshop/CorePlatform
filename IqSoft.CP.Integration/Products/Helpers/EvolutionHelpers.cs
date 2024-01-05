using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.Evolution;
using IqSoft.CP.Integration.Products.Models.IqSoft;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class EvolutionHelpers
    {
        private static readonly List<string> RestrictedCurrencies = new List<string> { Constants.Currencies.IranianTuman, Constants.Currencies.USDT };
        private static readonly List<string> RestrictedCountries = new List<string> { "IR" };
        public static string GetUrl(int productId, string token, int clientId, bool isForMobile, string ip, SessionIdentity session, ILog log)
        {
            if (string.IsNullOrEmpty(ip))
                ip = "127.0.0.1";

            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.Evolution);
            var client = CacheManager.GetClientById(clientId);
            var region = CacheManager.GetRegionById(client.RegionId, session.LanguageId);
            var product = CacheManager.GetProductById(productId);
            while (region.TypeId != (int)RegionTypes.Country)
            {
                if (region.ParentId == null)
                    break;
                region = CacheManager.GetRegionById(region.ParentId.Value, session.LanguageId);
            }
            if (region != null && !string.IsNullOrEmpty(region.IsoCode) && RestrictedCountries.Contains(region.IsoCode.ToUpper()))
            {
                region = null;
                ip = "127.0.0.1";
            }
            var casinoKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, gameProvider.Id, Constants.PartnerKeys.EvolutionCasinoKey);
            var apiToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, gameProvider.Id, Constants.PartnerKeys.EvolutionApiToken);
            var host = CacheManager.GetGameProviderValueByKey(client.PartnerId, gameProvider.Id, Constants.PartnerKeys.EvolutionHostName);
            if (Regex.IsMatch(host, "{\\d+}"))
                host = string.Format(host, session.Domain);
            var brandId = CacheManager.GetGameProviderValueByKey(client.PartnerId, gameProvider.Id, Constants.PartnerKeys.EvolutionBrandId);
            var url = string.Format(gameProvider.GameLaunchUrl, host, casinoKey, apiToken);
            string postBody;
            var liveCasinoPageUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.LiveCasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(liveCasinoPageUrl))
                liveCasinoPageUrl = string.Format("https://{0}/products/5", session.Domain);
            else
                liveCasinoPageUrl = string.Format(liveCasinoPageUrl, session.Domain);

            if (product.ExternalId == "lobby")
            {
                var authenticationInput = new
                {
                    uuid = token,
                    player = new
                    {
                        id = clientId.ToString(),
                        update = true,
                        firstName = string.IsNullOrEmpty(client.FirstName) ? clientId.ToString() : client.FirstName,
                        lastName = string.IsNullOrEmpty(client.LastName) ? clientId.ToString() : client.FirstName,
                        country = (region != null && !string.IsNullOrEmpty(region.IsoCode) && region.TypeId == (int)RegionTypes.Country) ? region.IsoCode : "uk",
                        language = session.LanguageId,
                        currency = RestrictedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
                        session = new
                        {
                            id = token,
                            ip,
                        },
                    },
                    config = new
                    {
                        brand = new
                        {
                            id = brandId,
                            skin = "1"
                        },
                        channel = new
                        {
                            wrapped = false,
                            mobile = isForMobile
                        },
                        urls = new
                        {
                            lobby = liveCasinoPageUrl
                        }
                    }
                };
                postBody = JsonConvert.SerializeObject(authenticationInput);
            }
            else
            {
                var authenticationInput = new
                {
                    uuid = token,
                    player = new
                    {
                        id = clientId.ToString(),
                        update = true,
                        firstName = string.IsNullOrEmpty(client.FirstName) ? clientId.ToString() : client.FirstName,
                        lastName = string.IsNullOrEmpty(client.LastName) ? clientId.ToString() : client.FirstName,
                        country = (region != null && !string.IsNullOrEmpty(region.IsoCode) && region.TypeId == (int)RegionTypes.Country) ? region.IsoCode : "uk",
                        language = session.LanguageId,
                        currency = RestrictedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
                        session = new
                        {
                            id = token,
                            ip,
                        },
                    },
                    config = new
                    {
                        brand = new
                        {
                            id = brandId,
                            skin = "1"
                        },
                        channel = new
                        {
                            wrapped = false,
                            mobile = isForMobile
                        },
                        game = new
                        {
                            table = new
                            {
                                id = product.ExternalId
                            }
                        },
                        urls = new
                        {
                            lobby = liveCasinoPageUrl
                        }
                    }
                };
                postBody = JsonConvert.SerializeObject(authenticationInput);
            }
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                PostData = postBody
            };
            log.Info("Evolution_GetUrl_" + JsonConvert.SerializeObject(httpRequestInput));
            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var output = JsonConvert.DeserializeObject<AuthenticationOutput>(resp);
            return string.Format("https://{0}{1}", host, output.Entry);
        }

        public static CurrentLiveGamesReport GetReportByRound(int clientId,  string roundId)
        {
            var client = CacheManager.GetClientById(clientId);
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.Evolution);
            var casinoKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, gameProvider.Id, Constants.PartnerKeys.EvolutionCasinoKey);
            var apiToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, gameProvider.Id, Constants.PartnerKeys.EvolutionGameHistoryApiToken);	            
            var mainPartner = CacheManager.GetPartnerById(Constants.MainPartnerId);
            var host = string.Format(CacheManager.GetGameProviderValueByKey(client.PartnerId, gameProvider.Id, Constants.PartnerKeys.EvolutionHostName), mainPartner.SiteUrl.Split(',')[0]);

           var requestHeaders = new Dictionary<string, string> {
                { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(casinoKey + ":" + apiToken)) } };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Get,
                RequestHeaders = requestHeaders,
                Url = string.Format("https://{0}/api/gamehistory/v1/players/{1}/games/{2}", host,clientId, roundId )
            };
            var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var reportOutput = JsonConvert.DeserializeObject<ReportOutput>(res);
            var response = new CurrentLiveGamesReport
            {
                RoundResult = new List<Data>()
            };
            var result = JObject.Parse(JsonConvert.SerializeObject(reportOutput.Data.Result));

            List<object> dealerCards = null;
            List<object> playerCards = null;
            try
            {
                dealerCards = result["dealer"]["cards"].ToObject<List<object>>();
                playerCards = result["player"]["cards"].ToObject<List<object>>();
            }
            catch {
                try
                {
                    dealerCards = result["dealerHand"].ToObject<List<object>>();
                    playerCards = result["dealtToPlayer"].ToObject<List<object>>();
                }
                catch { }
            }
            var data = new Data
            {
                RoundId = reportOutput.Data.RoundId,
                TableID = reportOutput.Data.Table.Id,
                TableName = reportOutput.Data.Table.Name,
                RoundDateTime = reportOutput.Data.RoundStartTime.ToString(),
                DealerName = reportOutput.Data.Dealer.Name,
                DealerId = reportOutput.Data.Dealer.Id,
                Results = JsonConvert.DeserializeObject<Round>(JsonConvert.SerializeObject(reportOutput.Data.Result))
            };
            data.Results.DealerCards = dealerCards;
            data.Results.PlayerCards = playerCards;
			data.Results.Participants = new List<object>() { reportOutput.Data.Participants };
			data.Results.Info = reportOutput.Data.Result;
            response.RoundResult.Add(data);
            return response;
        }
    }
}
