using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using System.Collections.Generic;
using IqSoft.CP.Integration.Products.Models.EveryMatrix;
using Newtonsoft.Json;
using System.Linq;
using IqSoft.CP.DAL.Models;
using System;
using IqSoft.CP.Common.Enums;
using log4net;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models.Cache;
using System.Net.Http;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class EveryMatrixHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.EveryMatrix);
        public static List<GameItem> GetGames(int partnerId)
        {
            var gamesUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixGamesUrl);
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Get,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = string.Format("{0}?types=game", gamesUrl)
            };
            var result = JsonConvert.DeserializeObject<List<GameItem>>(string.Format("[{0}]", CommonFunctions.SendHttpRequest(httpRequestInput, out _)
                                                                                       .Replace("}\r\n{", "},{")))
                              .Where(x => x.data != null && x.data.enabled).ToList();

            httpRequestInput.Url = string.Format("{0}?types=table", gamesUrl);
            result.AddRange(JsonConvert.DeserializeObject<List<GameItem>>(string.Format("[{0}]", CommonFunctions.SendHttpRequest(httpRequestInput, out _)
                                                                                       .Replace("}\r\n{", "},{")))
                              .Where(x => x.data != null && x.data.enabled).ToList());
            return result;
        }

        public static string GetOddsMatrixUrl(string token, int partnerId, bool isForDemo, SessionIdentity session)
        {
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixOperatorId);
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = string.Format("{0}/Loader/start/{1}/sports-betting", Provider.GameLaunchUrl, operatorId)
            };
            if (isForDemo)
                httpRequestInput.PostData =  JsonConvert.SerializeObject(new { language = session.LanguageId });
            else
            {
                var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
                if (string.IsNullOrEmpty(casinoPageUrl))
                    casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
                else
                    casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

                var input = new
                {
                    _sid = token,
                    language = session.LanguageId,
                    funMode = false,
                    casinolobbyurl = casinoPageUrl
                };
                httpRequestInput.PostData = JsonConvert.SerializeObject(input);
            }

            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            try
            {
                var resp = JsonConvert.DeserializeObject<GameSessionOutput>(response);
                if (resp.Success)
                    return resp.SessionId;
                return resp.ErrorMessage;
            }
            catch
            {
                return response;
            }
        }

        public static void RegisterPlayerForCEDirect(int clientId, SessionIdentity session, ILog log)
        {
            try
            {
                var client = CacheManager.GetClientById(clientId);
                var adapterUrl = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixAdapterUrl);
                var operatorId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixOperatorId);
                var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixAdapterApiKey);
                var gender = client.Gender;
                if (!Enum.IsDefined(typeof(Gender), client.Gender))
                    gender = (int)Gender.Male;

                using (var regionBl = new RegionBll(session, log))
                {
                    var regionPath = regionBl.GetRegionPath(client.RegionId);
                    var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
                    if (country == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                    var input = new
                    {
                        countryAlpha3Code = country.IsoCode3,
                        gender = ((Gender)client.Gender).ToString(),
                        alias = client.UserName,
                        city = client.City,
                        lang = session.LanguageId,
                        currency = client.CurrencyId,
                        firstName = client.FirstName,
                        lastName = client.LastName,
                        operatorUserId = client.Id.ToString(),
                        userName = client.UserName,
                        email = client.Email,
                        birthDate = client.BirthDate
                    };

                    var httpRequestInput = new HttpRequestInput
                    {
                        RequestMethod = HttpMethod.Post,
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestHeaders = new Dictionary<string, string> { { "X-Api-Key", apiKey }, { "X-Tenant-ID", operatorId } },
                        Url = adapterUrl,
                        PostData = JsonConvert.SerializeObject(input)
                    };

                    var response = JsonConvert.DeserializeObject<BaseOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    if (!string.IsNullOrEmpty(response.Message))
                        throw new Exception(response.Message);
                }
            }
            catch (Exception ex)
            {
                log.Error("EM External Adapter Error: " + ex.Message);

            }
        }

        public static decimal GetPlayerBonusBalance(BllClient client, ILog log)
        {
            try
            {
                var bonusApiUrl = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixBonusAPIUrl);
                var domainID = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixOperatorId);
                var authToken = GetBonusAuthToken(client.PartnerId);
                var input = new
                {
                    domainID,
                    userID = client.Id,
                    currency = client.CurrencyId
                };
                var inputData = CommonFunctions.GetUriDataFromObject(input);
                var httpRequestInput = new HttpRequestInput
                {
                    RequestMethod = HttpMethod.Get,
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", authToken } },
                    Url = $"{bonusApiUrl}/wallet/aggregation?{inputData}"
                };

                var response = JsonConvert.DeserializeObject<BonusWalletOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (!string.IsNullOrEmpty(response.Message))
                    throw new Exception(response.Message);
                return response.BonusWallets.Values.Select(x => x.Amount).DefaultIfEmpty(0).Sum();
            }
            catch (Exception ex)
            {
                log.Error("EM External Adapter Error: " + ex.Message);
            }
            return 0m;
        }

        public static List<ApiBonusModule> GetPlayerBonuses(BllClient client, ILog log)
        {
            try
            {
                var bonusApiUrl = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixBonusAPIUrl);
                var domainID = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixOperatorId);
                var authToken = GetBonusAuthToken(client.PartnerId);
                var httpRequestInput = new HttpRequestInput
                {
                    RequestMethod = HttpMethod.Post,
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", authToken } },
                    Url = $"{bonusApiUrl}/wallet/{domainID}/{client.Id}?maxRecords=100"
                };

                var bonuses = JsonConvert.DeserializeObject<BonusListOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                return bonuses.Wallets.Select(x => new ApiBonusModule
                {
                    Amount = x.Amount,
                    RemainingWagerRequirement = x.Amount,//x.GrantedBonusAmount,
                    InitialWagerRequirement = x.Extension.TotalWR,
                    BonusType = x.Type,
                    GrantedTime = x.Ins,
                    Status = x.Status
                }).ToList();


                // check response here


            }
            catch (Exception ex)
            {
                log.Error("EM Bonuses Error: " + ex.Message);

            }
            return new List<ApiBonusModule>();
        }

        private static string GetBonusAuthToken(int partnerId)
        {
            var bonusAppUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixBonusAppUrl);
            var username = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixBonusApiUsername);
            var password = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixBonusApiPassword);

            var input = new { username, password };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                Url = bonusAppUrl,
                PostData = CommonFunctions.GetUriDataFromObject(input)
            };
            return JsonConvert.DeserializeObject<BonusLoginOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).SessionId;
        }
    }
}