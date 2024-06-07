using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using Newtonsoft.Json;
using IqSoft.CP.Integration.Platforms.Models.KRA;
using System.Collections.Generic;
using System;
using System.Linq;
using log4net;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public static class KRAHelpers
    {
        private static readonly string TaxType = "EXCISE";
        private static string GetAuthenticationToken(int partnerId)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAApiUrl).StringValue;
            var username = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAApiUsername).StringValue;
            var password = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAApiPassword).StringValue;
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = $"{url}/authenticate",
                PostData = JsonConvert.SerializeObject(new { username, password })
            };
            return JsonConvert.DeserializeObject<AuthOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).IdToken;
        }

        public static void SendPaymentsInfo(int partnerId, decimal amount, DateTime paymentDate, ILog log)
        {
            try
            {
                var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAApiUrl).StringValue;
                var operatorPin = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAOperatorPin).StringValue;
                var token = GetAuthenticationToken(partnerId);
                var transactionDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
                var requestInput = new
                {
                    hash = CommonFunctions.ComputeSha256($"{operatorPin}{transactionDate}EXCISE"),
                    paymentInfo = new
                    {
                        pinNo = operatorPin,
                        amount,
                        taxType = TaxType,
                        transactionDate,
                        periodFrom = paymentDate.ToString("yyyy-MM-dd"),
                        periodTo = paymentDate.AddDays(1).ToString("yyyy-MM-dd")
                    }
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Bearer {token}" } },
                    Url = $"{url}/generatePrnRequest",
                    PostData = JsonConvert.SerializeObject(requestInput)
                };
                log.Info("generatePrnRequest: " + JsonConvert.SerializeObject(httpRequestInput));
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var baseOutput = JsonConvert.DeserializeObject<BaseOutput>(resp);
                if (baseOutput.Response?.RESULT?.ResponseCode != "1111")
                    log.Error(resp);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static void SendBetsInfo(int partnerId, List<BetItem> bets, ILog log)
        {
            try
            {
                var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAApiUrl).StringValue;
                var operatorPin = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAOperatorPin).StringValue;
                var taxFee = Convert.ToDecimal(CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRATaxFee).StringValue) / 100;
                var token = GetAuthenticationToken(partnerId);
                var transactionDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
                var requestInput = new
                {
                    hash = CommonFunctions.ComputeSha256($"{operatorPin}{transactionDate}{bets.Count()}"),
                    header = new
                    {
                        operatorPin,
                        transactionDate,
                        noOfStakes = bets.Count()
                    },
                    details = bets.Select(x => new
                    {
                        betId = x.BetId,
                        customerId = x.ClientId.ToString(),
                        mobileNo = x.MobileNumber,
                        punterAmt = x.BetAmount,
                        stakeAmt = x.BetAmount*taxFee,
                        desc = string.Empty,
                        odds = x.Coefficent,
                        stakeType = x.IsSport ? "NORMAL" : "VIRTUAL",
                        dateOfStake = x.PlacementTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        exciseAmt = x.BetAmount,
                        expectedOutcomeTime = x.CalculationDate?.ToString("yyyy-MM-ddTHH:mm:ss") ?? x.PlacementTime.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss"),
                        walletBalanceStake = x.CurrentBalance
                    }).ToArray()
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Bearer {token}" } },
                    Url = $"{url}/receiveStakeData",
                    PostData = JsonConvert.SerializeObject(requestInput)
                };
                log.Info("receiveStakeData: " + JsonConvert.SerializeObject(httpRequestInput));
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var baseOutput = JsonConvert.DeserializeObject<BaseOutput>(resp);
                if (baseOutput.Response?.RESULT?.ResponseCode != "1111")
                    log.Error(resp);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static void SendWinsInfo(int partnerId, List<BetItem> wins, ILog log)
        {
            try
            {
                var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAApiUrl).StringValue;
                var operatorPin = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAOperatorPin).StringValue;
                var taxFee = Convert.ToDecimal(CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRATaxFee).StringValue) / 100;
                var token = GetAuthenticationToken(partnerId);
                var transactionDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
                var requestInput = new
                {
                    hash = CommonFunctions.ComputeSha256($"{operatorPin}{transactionDate}{wins.Count()}"),
                    header = new
                    {
                        operatorPin,
                        transactionDate,
                        noOfOutcomes = wins.Count()
                    },
                    details = wins.Select(x => new
                    {
                        outcomeInfo = new
                        {
                            betId = x.BetId,
                            outcome = x.WinAmount > 0 ? "WIN" : "LOSE",
                            outcomedate = x.CalculationDate?.ToString("yyyy-MM-ddTHH:mm:ss"),
                            payout = x.WinAmount,
                            winnings = x.WinAmount,
                            withholdingTax = x.WinAmount*taxFee,
                            walletBalanceOutcome = x.CurrentBalance
                        }
                    }).ToArray()
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Bearer {token}" } },
                    Url = $"{url}/receiveOutcomeData",
                    PostData = JsonConvert.SerializeObject(requestInput)
                };
                log.Info("receiveOutcomeData: " + JsonConvert.SerializeObject(httpRequestInput));
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var baseOutput = JsonConvert.DeserializeObject<BaseOutput>(resp);
                if (baseOutput.Response?.RESULT?.ResponseCode != "1111")
                    log.Error(resp);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
