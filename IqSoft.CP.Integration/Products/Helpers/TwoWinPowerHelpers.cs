using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Products.Models.TwoWinPower;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class TwoWinPowerHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.TwoWinPower);

        public static List<string> UnsuppordedCurrenies = new List<string>
        {
            Constants.Currencies.TurkishLira
        };
        public static void SetFreespin(int clientId, int clientBonusId, int freeSpinId, ILog log)
        {
            using (var bonusBl = new BonusService(new SessionIdentity(), log))
            {
                using (var documentBl = new DocumentBll(bonusBl))
                {
                    var client = CacheManager.GetClientById(clientId);
                    var bonus = bonusBl.GetBonusById(freeSpinId);
                    var bonusInfo = JsonConvert.DeserializeObject<FreeSpinBonus>(bonus.Info);
                    var freeSpins = bonusInfo.FreeSpins.FirstOrDefault();
                    if (freeSpins == null)
                        throw BaseBll.CreateException(null, Constants.Errors.BonusNotFound);
                    var clientBonus = documentBl.GetClientBonuses(client.Id).FirstOrDefault(x => x.Id == clientBonusId);
                    var product = CacheManager.GetProductById(bonusInfo.ProductId);
                    if (product == null)
                        throw BaseBll.CreateException(null, Constants.Errors.ProductNotFound);

                    var merchantId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.TwoWinPowerId);
                    var secretKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.TwoWinPowerSecretKey);
                    Random random = new Random();
                    var randomLine = random.Next(0, freeSpins.FreeSpinBets.Count());
                    var timestamp = CommonFunctions.GetCurrentUnixTimestampSeconds().ToString();
                    var randomString = CommonFunctions.ComputeMd5(CommonFunctions.GetRandomString(10));

                    var finishTime = (long)(clientBonus.CalculationTime.Value - CommonFunctions.UnixEpoch).TotalSeconds;
                    var maxStartTime = (long)(DateTime.UtcNow.AddMinutes(1) - CommonFunctions.UnixEpoch).TotalSeconds;
                    var startTime = (long)(clientBonus.CreationTime - CommonFunctions.UnixEpoch).TotalSeconds;
                    if (startTime > maxStartTime)
                        startTime = maxStartTime;
                    if (finishTime < maxStartTime)
                        throw BaseBll.CreateException(null, Constants.Errors.InvalidDataRange);
                    var currency = client.CurrencyId;
                    if (UnsuppordedCurrenies.Contains(currency))
                        currency = Constants.Currencies.USADollar;
                    var freespinInput = new FreespinInput
                    {
                        game_uuid = product.ExternalId,
                        player_id = clientId.ToString(),
                        player_name = client.Id.ToString(),
                        currency = currency,
                        quantity = bonus.TurnoverCount.Value,
                        denomination = freeSpins.Denominations[0],
                        bet_id = freeSpins.FreeSpinBets[randomLine].Id,
                        freespin_id = clientBonusId.ToString(),
                        valid_from = ((long)(DateTime.UtcNow.AddMinutes(1) - CommonFunctions.UnixEpoch).TotalSeconds).ToString(),
                        valid_until = ((long)(DateTime.UtcNow.AddHours(1) - CommonFunctions.UnixEpoch).TotalSeconds).ToString()
                    };

                    SortedDictionary<string, string> requestHeaders = new SortedDictionary<string, string>
                {
                    { "X-Merchant-Id", merchantId},
                    { "X-Timestamp", timestamp },
                    { "X-Nonce", randomString }
                };
                    var body = CommonFunctions.GetSortedParamWithValuesAsString(freespinInput, "&");
                    var sign = body;
                    sign += requestHeaders.Aggregate(string.Empty, (current, par) => current + "&" + par.Key + "=" + par.Value);
                    requestHeaders.Add("X-Sign", CommonFunctions.ComputeHMACSha1(sign, secretKey).ToLower());

                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = Provider.GameLaunchUrl + "freespins/set",
                        RequestHeaders = new Dictionary<string, string>(requestHeaders),
                        PostData = body
                    };
                    CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                }
            }
        }

        public static FreespinsOutput GetFreespinBets(int partnerId, string productExternalId, string currency)
        {
            var merchantId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TwoWinPowerId);
            var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TwoWinPowerSecretKey);

            var freespinInput = new
            {
                game_uuid = productExternalId,
                currency
            };
            var timestamp = CommonFunctions.GetCurrentUnixTimestampSeconds().ToString();
            var randomString = CommonFunctions.ComputeMd5(CommonFunctions.GetRandomString(10));
            SortedDictionary<string, string> requestHeaders = new SortedDictionary<string, string>
            {
                { "X-Merchant-Id", merchantId},
                { "X-Timestamp", timestamp },
                { "X-Nonce", randomString }
            };
            var body = CommonFunctions.GetSortedParamWithValuesAsString(freespinInput, "&");
            var sign = body;
            sign += requestHeaders.Aggregate(string.Empty, (current, par) => current + "&" + par.Key + "=" + par.Value);
            sign = CommonFunctions.ComputeHMACSha1(sign, secretKey).ToLower();
            requestHeaders.Add("X-Sign", sign);

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Get,
                Url = string.Format("{0}{1}?{2}", Provider.GameLaunchUrl, "freespins/bets", body),
                RequestHeaders = new Dictionary<string, string>(requestHeaders)
            };
            return JsonConvert.DeserializeObject<FreespinsOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
        }

        public static void GetAllFreespins(int partnerId)
        {
            var result = new List<FreeSpinBonus>();
            using (var db = new IqSoftCorePlatformEntities())
            {
                var providerProducts = db.Products.Where(x => x.GameProviderId == Provider.Id).Select(x => new { x.Id, x.ExternalId }).ToList();
                var currencies = db.PartnerCurrencySettings.Where(x => x.PartnerId == partnerId && x.State == (int)PartnerCurrencyStates.Active).ToList();

                foreach (var product in providerProducts)
                    foreach (var curr in currencies)
                    {
                        var resp = GetFreespinBets(partnerId, product.ExternalId, curr.CurrencyId);
                        result.Add(
							new FreeSpinBonus
							{
							    ProductId = product.Id,
							    FreeSpins = new List<FreeSpin>
							    {
							        new FreeSpin
							        {
							            Currency = curr.CurrencyId,
							            Denominations = resp.Denominations,
							            FreeSpinBets = new List<FreeSpinBet>( resp.FreespinBets.Select(x=> new FreeSpinBet
							                                                                                {
							                                                                                    Id = x.BetId,
							                                                                                    Line  =x.BetPerLine
							                                                                                }).ToList())
							        }
							    }
							}
                        );
                    }
            }
        }

        public static string GetSessionUrl(int partnerId, int clientId, string token, int productId, bool isForDemo, SessionIdentity session)
        {
            var product = CacheManager.GetProductById(productId);
            if (product == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ProductNotFound);
            var path = "games/init";
            if (isForDemo)
                path += "-demo";
            var partner = CacheManager.GetPartnerById(partnerId);
            if (partner == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerNotFound);
            BllClient client = null;
            if (!isForDemo)
            {
                client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                if (client.PartnerId != partnerId)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPartnerId);
            }

            var merchantId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TwoWinPowerId);
            var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TwoWinPowerSecretKey);
            var currency = (client == null ? partner.CurrencyId : client.CurrencyId);
            if (UnsuppordedCurrenies.Contains(currency))
                currency = Constants.Currencies.USADollar;
            var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
            if (string.IsNullOrEmpty(casinoPageUrl))
                casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
            else
                casinoPageUrl = string.Format(casinoPageUrl, session.Domain);

            var requestInput = new TwoWinPowerInput
            {
                session_id = token,
                game_uuid = product.ExternalId,
                currency = currency,
                player_id = clientId.ToString(),
                player_name = client == null ? "demo" : client.Id.ToString(),
                return_url = casinoPageUrl
            };
            var timestamp = CommonFunctions.GetCurrentUnixTimestampSeconds().ToString();
            var randomString = CommonFunctions.ComputeMd5(CommonFunctions.GetRandomString(10));
            SortedDictionary<string, string> requestHeaders = new SortedDictionary<string, string>
                {
                    { "X-Merchant-Id", merchantId},
                    { "X-Timestamp", timestamp },
                    { "X-Nonce", randomString }
                };
            var body = CommonFunctions.GetSortedParamWithValuesAsString(requestInput, "&");
            var sign = body;
            sign += requestHeaders.Aggregate(string.Empty, (current, par) => current + "&" + par.Key + "=" + par.Value);

            requestHeaders.Add("X-Sign", CommonFunctions.ComputeHMACSha1(sign, secretKey).ToLower());
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = Provider.GameLaunchUrl + path,
                RequestHeaders = new Dictionary<string, string>(requestHeaders),
                PostData = body
            };

            var resp = JsonConvert.DeserializeObject<TwoWinPowerResponse>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            return resp.url;
        }

        public static void SelfValidate(int partnerId, int productId)
        {
            var product = CacheManager.GetProductById(productId);
            var merchantId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TwoWinPowerId);
            var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TwoWinPowerSecretKey);

            var timestamp = CommonFunctions.GetCurrentUnixTimestampSeconds().ToString();
            var randomString = CommonFunctions.ComputeMd5(CommonFunctions.GetRandomString(10));
            SortedDictionary<string, string> requestHeaders = new SortedDictionary<string, string>
                {
                    { "X-Merchant-Id", merchantId},
                    { "X-Timestamp", timestamp },
                    { "X-Nonce", randomString }
                };

            var sign = requestHeaders.Aggregate(string.Empty, (current, par) => current + par.Key + "=" + par.Value + "&");
            sign = sign.Remove(sign.LastIndexOf("&"), 1);
            requestHeaders.Add("X-Sign", CommonFunctions.ComputeHMACSha1(sign, secretKey).ToLower());

            var path = "self-validate";
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = Provider.GameLaunchUrl + path,
                RequestHeaders = new Dictionary<string, string>(requestHeaders)
            };
            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

        public static void GetFreeSpinInfo(int partnerId, int freeSpinId)
        {
            var merchantId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TwoWinPowerId);
            var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TwoWinPowerSecretKey);

            var freespinInput = new
            {
                freespin_id = freeSpinId
            };
            var timestamp = CommonFunctions.GetCurrentUnixTimestampSeconds().ToString();
            var randomString = CommonFunctions.ComputeMd5(CommonFunctions.GetRandomString(10));
            SortedDictionary<string, string> requestHeaders = new SortedDictionary<string, string>
            {
                { "X-Merchant-Id", merchantId},
                { "X-Timestamp", timestamp },
                { "X-Nonce", randomString }
            };
            var body = CommonFunctions.GetSortedParamWithValuesAsString(freespinInput, "&");
            var sign = body;
            sign += requestHeaders.Aggregate(string.Empty, (current, par) => current + "&" + par.Key + "=" + par.Value);
            sign = CommonFunctions.ComputeHMACSha1(sign, secretKey).ToLower();
            requestHeaders.Add("X-Sign", sign);

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Get,
                Url = string.Format("{0}{1}?{2}", Provider.GameLaunchUrl, "freespins/get", body),
                RequestHeaders = new Dictionary<string, string>(requestHeaders)
            };
            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

        public static void CancelFreeSpin(int partnerId, int freeSpinId)
        {
            var merchantId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TwoWinPowerId);
            var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TwoWinPowerSecretKey);
            var timestamp = CommonFunctions.GetCurrentUnixTimestampSeconds().ToString();
            var randomString = CommonFunctions.ComputeMd5(CommonFunctions.GetRandomString(10));

            var freespinInput = new
            {
                freespin_id = freeSpinId
            };

            SortedDictionary<string, string> requestHeaders = new SortedDictionary<string, string>
                {
                    { "X-Merchant-Id", merchantId},
                    { "X-Timestamp", timestamp },
                    { "X-Nonce", randomString }
                };
            var body = CommonFunctions.GetSortedParamWithValuesAsString(freespinInput, "&");
            var sign = body;
            sign += requestHeaders.Aggregate(string.Empty, (current, par) => current + "&" + par.Key + "=" + par.Value);
            requestHeaders.Add("X-Sign", CommonFunctions.ComputeHMACSha1(sign, secretKey).ToLower());

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = Provider.GameLaunchUrl + "freespins/cancel",
                RequestHeaders = new Dictionary<string, string>(requestHeaders),
                PostData = body
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
    }
}
