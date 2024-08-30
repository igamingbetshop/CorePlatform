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
using System.Text;
using IqSoft.CP.Common.Models.Bonus;
using YamlDotNet.Core.Tokens;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class EveryMatrixHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.EveryMatrix);
        private static readonly Dictionary<string, int> StatusKeys = new Dictionary<string, int>
        {
            {"active", (int)ClientBonusStatuses.Active },
            {"expired", (int)ClientBonusStatuses.Expired },
            {"forfeited", (int)ClientBonusStatuses.Closed},
            {"released", (int)ClientBonusStatuses.Finished },
            {"completed", (int)ClientBonusStatuses.Finished },
            {"closed", (int)ClientBonusStatuses.Closed },
        };
        public static List<GameItem> GetGames(int partnerId)
        {
            var gamesUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixGamesUrl);
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = string.Format("{0}?types=game", gamesUrl)
            };
            var result = JsonConvert.DeserializeObject<List<GameItem>>(string.Format("[{0}]", CommonFunctions.SendHttpRequest(httpRequestInput, out _)
                                                                                       .Replace("}\r\n{", "},{")))
                              .Where(x => x.Data != null && x.Data.Enabled).ToList();

            httpRequestInput.Url = string.Format("{0}?types=table", gamesUrl);
            result.AddRange(JsonConvert.DeserializeObject<List<GameItem>>(string.Format("[{0}]", CommonFunctions.SendHttpRequest(httpRequestInput, out _)
                                                                                       .Replace("}\r\n{", "},{")))
                              .Where(x => x.Data != null && x.Data.Enabled).ToList());
            return result;
        }

        public static string GetOddsMatrixUrl(string token, int partnerId, bool isForDemo, SessionIdentity session, ILog log)
        {
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixOperatorId);
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
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
                var regionPath = CacheManager.GetRegionPathById(client.RegionId);
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
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestHeaders = new Dictionary<string, string> { { "X-Api-Key", apiKey }, { "X-Tenant-ID", operatorId } },
                    Url = adapterUrl,
                    PostData = JsonConvert.SerializeObject(input)
                };

                var currentTime = DateTime.UtcNow;
                var response = JsonConvert.DeserializeObject<BaseOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                
                if (!string.IsNullOrEmpty(response.Message))
                    throw new Exception(response.Message);
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
                    RequestMethod = Constants.HttpRequestMethods.Get,
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
                    RequestMethod = Constants.HttpRequestMethods.Get,
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", authToken } },
                    Url = $"{bonusApiUrl}/wallet/{domainID}/{client.Id}?maxRecords=100"
                };

                var bonuses = JsonConvert.DeserializeObject<BonusListOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                return bonuses.Wallets.Select(x => new ApiBonusModule
                {
                    Id = x.Id,
//                    Amount = x.Amount,
                    RemainingWagerRequirement = x.Amount,//x.GrantedBonusAmount,
                    InitialWagerRequirement = x.Extension.TotalWR,
                    BonusType = x.Type,
                    GrantedTime = x.Ins,
                    Status = x.Status, 
                    StatusId = StatusKeys[x.Status.ToLower()],
                    TypeId = (int)BonusTypes.CampaignWagerSport
                }).ToList();

            }
            catch (Exception ex)
            {
                log.Error("EM Bonuses Error: " + ex.Message);

            }
            return new List<ApiBonusModule>();
        }

        public static void ForfeitBonusWallet(BllClient client, string bonusWalletId, ILog log)
        {
            var bonusApiUrl = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixBonusAPIUrl);
            var domainID = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixOperatorId);
            var authToken = GetBonusAuthToken(client.PartnerId);
            var input = new
            {
                domainID,
                userID = client.Id,
                bonusWalletID = bonusWalletId
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Delete,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", authToken } },
                Url = $"{bonusApiUrl}/wallet?{CommonFunctions.GetUriDataFromObject(input)}"
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

        private static string GetBonusAuthToken(int partnerId)
        {
            var bonusAppUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixBonusAppUrl);
            var username = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixBonusApiUsername);
            var password = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixBonusApiPassword);

            var input = new { username, password };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                Url = bonusAppUrl,
                PostData = CommonFunctions.GetUriDataFromObject(input)
            };
            return JsonConvert.DeserializeObject<BonusLoginOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).SessionId;
        }

        private static readonly Dictionary<string, string> Vendors = new Dictionary<string, string>
        {
            {"ELKStudios", "ELK"},
            {"PlaysonDirect", "Playson"},
            {"PushGaming", "Push"},
            {"SpearheadStudios", "RGS_Matrix"},
            {"2BY2", "Microgaming"},
            {"AdoptItPublishing", "Microgaming"},
            {"AlchemyGaming", "Microgaming"},
            {"All41Studios", "Microgaming"},
            {"Aurum", "Microgaming"},
            {"Foxium", "Microgaming"},
            {"GachaStudios", "Microgaming"},
            {"GameburgerStudios", "Microgaming"},
            {"BuckStakesEntertainment", "Microgaming"},
            {"CrazyTooth", "Microgaming"},
            {"FortuneFactory", "Microgaming"},
            {"GoldCoinStudios", "Microgaming"},
            {"GongGaming", "Microgaming"},
            {"InfinityDragonStudios", "Microgaming"},
            {"JadeRabbitStudios", "Microgaming"},
            {"Jftw", "Microgaming"},
            {"JustForTheWin", "Microgaming"},
            {"LightningBox", "Microgaming"},
            {"Live5Gaming", "Microgaming"},
            {"NekoGames", "Microgaming"},
            {"NeonValleyStudios", "Microgaming"},
            {"NorthernLights", "Microgaming"},
            {"OldSkool", "Microgaming"},
            {"PearFiction", "Microgaming"},
            {"PlankGaming", "Microgaming"},
            {"Pulse8", "Microgaming"},
            {"RabcatGambling", "Microgaming"},
            {"RealDealerStudios", "Microgaming"},
            {"Realistic", "Microgaming"},
            {"Slingshot", "Microgaming"},
            {"SnowbornGames", "Microgaming"},
            {"SpinPlayGames", "Microgaming"},
            {"StormcraftStudios", "Microgaming"},
            {"TripleEdgeStudios", "Microgaming"},
            {"4ThePlayer", "RelaxGaming"},
            {"Arcadem", "Oryx"},
            {"AtomicLab", "Oryx"},
            {"ArmadilloStudios", "RGS_Matrix"},

 //{"BigTimeGaming", "Microgaming,RelaxGaming"},
 //{"GoldenRockStudios", "Microgaming,RelaxGaming"},
 //{"StormGames", "Microgaming,RelaxGaming"}
        };
        public static bool AwardFreeSpin( FreeSpinModel freeSpinModel, ILog log)
        {
            var client = CacheManager.GetClientById(freeSpinModel.ClientId);
            var product = CacheManager.GetProductByExternalId(Provider.Id, freeSpinModel.ProductExternalId);
            if (!product.SubProviderId.HasValue)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
            var vendor = CacheManager.GetGameProviderById(product.SubProviderId.Value).Name;
            if (Vendors.ContainsKey(vendor))
                vendor = Vendors[vendor];
            else if (freeSpinModel.ProductExternalId.ToLower().Contains("microgaming"))
                vendor = "Microgaming";
            else if (freeSpinModel.ProductExternalId.ToLower().Contains("relaxgaming"))
                vendor = "RelaxGaming";
            var vendorApiUrl = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixVendorApiUrl);
            var domainID = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixOperatorId);
            var username = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixFSBonusApiUsername);
            var password = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixFSBonusApiPassword);
            var region = CacheManager.GetRegionById(client.RegionId, Constants.DefaultLanguageId);
            var requestHeaders = new Dictionary<string, string>
            { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{username}:{password}")) } };
            var externalIds = freeSpinModel.ProductExternalId.Split(',');


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

            var input = new
            {
                BonusSource = 2,
                OperatorUserId = freeSpinModel.ClientId.ToString(),
                GameIds = new List<string> { externalIds.Last() },
                NumberOfFreeRounds = freeSpinModel.SpinCount,
                BonusId = freeSpinModel.BonusId.ToString(),
                FreeRoundsEndDate = freeSpinModel.FinishTime.ToString("MM/dd/yyyy HH:MM:ss tt"),
                DomainId = domainID,
                UserDetails = new
                {
                    CountryAlpha3Code = region.IsoCode3,
                    Gender = ((Gender)client.Gender).ToString(),
                    Alias = client.UserName,
                    Currency = client.CurrencyId,
                    City = client.City, 
                    client.FirstName,
                    client.LastName,
                    OperatorUserId = client.Id.ToString(),
                    Lang = client.LanguageId.ToUpper()
                },
                AdditionalParameters = new
                {
                    Currency  = client.CurrencyId,
                    CoinValue = (int?)freeSpinModel.CoinValue,
                    //LineCount = 0,
                    //CampaignId = freeSpinModel.BonusId.ToString(),
                    Lines = (int?)freeSpinModel.Lines,
                    LineCount = (int?)freeSpinModel.Lines,
                    Coins = (int?)freeSpinModel.Coins,
                    BetValueLevel = (int?)bValue,
                    //CoinSize = freeSpinModel.CoinValue / freeSpinModel.Lines,
                    //SpinCoinPosition = 0,
                    //BetLine = freeSpinModel.BetValueLevel, //??
                    //BetValue = freeSpinModel.BetValueLevel, //??
                    //BetLevel = freeSpinModel.BetValueLevel, //??
                    Denomination =(int?)freeSpinModel.Coins, 
                    BetAmount = (int?)bValue,
                    Value = freeSpinModel.BetValues,
                    betPerLine = freeSpinModel.BetValues,
                    SpinCoinPosition = (int?)freeSpinModel.Coins,
                    BetValue = freeSpinModel.BetValues,
                    Bet = freeSpinModel.BetValues
                }
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = requestHeaders,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = $"{vendorApiUrl}/vendorBonus/{vendor}/AwardBonus",
                PostData = JsonConvert.SerializeObject(input)
            };
            log.Info("EM_FreeSpins_Input" + JsonConvert.SerializeObject(httpRequestInput));
            var r = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            log.Info("EM_FreeSpins_Response" + r);
            var response = JsonConvert.DeserializeObject<FreeSpinOutput>(r);
            if (!response.Success)
            {
                log.Error(r);
                return false;
            }
            return true;
               
        }

        public static void ForfeitFreeSpinBonus(int clientId, int bonusId, int productId)
        {
            var client = CacheManager.GetClientById(clientId);
            var product = CacheManager.GetProductById(productId);
            if (!product.SubProviderId.HasValue)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
            var vendor = CacheManager.GetGameProviderById(product.SubProviderId.Value).Name;
            var vendorApiUrl = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixVendorApiUrl);
            var domainID = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixOperatorId);
            var username = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixBonusApiUsername);
            var password = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.EveryMatrixBonusApiPassword);
            var requestHeaders = new Dictionary<string, string>
            { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{username}:{password}")) } };
            var input = new
            {
                BonusSource = 2,
                Comment = "ForfeitFreeSpinBonus",
                OperatorUserId = clientId.ToString(),
                BonusId = bonusId,
                DomainId = domainID,
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = requestHeaders,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = $"{vendorApiUrl}/vendorBonus{vendor}/ForfeitBonusHTTP/1.1",
                PostData = JsonConvert.SerializeObject(input)
            };
            var response = JsonConvert.DeserializeObject<FreeSpinOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (response.ErrorCode != 0)
                throw new Exception($"ErrorCode: {response.ErrorCode}, ErrorMessage: {response.ErrorName}, VendorError: {response.VendorError}");
        }
    }
}