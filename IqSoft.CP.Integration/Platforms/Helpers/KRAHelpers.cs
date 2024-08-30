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
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public static class KRAHelpers
    {
        private static readonly string TaxType = "EXCISE";
        private static readonly string WithHoldingTaxType = "IT_WHT_0222";

        public static void BroadcastBettingReport(int partnerId, List<BetItem> bets, ILog log)
        {
            try
            {
                var token = GetAuthenticationToken(partnerId);
                var totalExciseTax = SendBetsInfo(partnerId, bets, token, log);
                var totalWithHoldingTax = SendWinsInfo(partnerId, bets, token, log);
                using (var partnerBl = new PartnerBll(new SessionIdentity(), log))
                {
                    var currentDate = DateTime.Today;
                    var dailyExciseTax = partnerBl.GetPartnerKey(partnerId, Constants.PartnerKeys.KRADailyExciseTax);
                    dailyExciseTax.NumericValue = (dailyExciseTax.NumericValue ?? 0)  + totalExciseTax;
                    if (!dailyExciseTax.DateValue.HasValue)
                        dailyExciseTax.DateValue = currentDate;
                    partnerBl.UpdatePartnerKey(dailyExciseTax);
                    if (dailyExciseTax.DateValue == null || (currentDate - dailyExciseTax.DateValue).Value.TotalDays > 0)
                    {
                        SendPrnInfo(partnerId, token, TaxType, dailyExciseTax.NumericValue.Value, dailyExciseTax.DateValue.Value, currentDate, log);
                        dailyExciseTax.NumericValue = 0;
                        dailyExciseTax.DateValue = currentDate;
                        partnerBl.UpdatePartnerKey(dailyExciseTax);
                    }
                    var dailyWithHoldingTax = partnerBl.GetPartnerKey(partnerId, Constants.PartnerKeys.KRADailyWithHoldingTax);
                    dailyWithHoldingTax.NumericValue = (dailyWithHoldingTax.NumericValue ?? 0)  + totalWithHoldingTax;
                    if (!dailyWithHoldingTax.DateValue.HasValue)
                        dailyWithHoldingTax.DateValue = currentDate;
                    partnerBl.UpdatePartnerKey(dailyWithHoldingTax);
                    if (dailyWithHoldingTax.DateValue == null || (currentDate - dailyWithHoldingTax.DateValue).Value.TotalDays > 0)
                    {
                        SendPrnInfo(partnerId, token, WithHoldingTaxType, dailyWithHoldingTax.NumericValue.Value, dailyWithHoldingTax.DateValue.Value, currentDate, log);
                        dailyWithHoldingTax.NumericValue = 0;
                        dailyWithHoldingTax.DateValue = currentDate;
                        partnerBl.UpdatePartnerKey(dailyWithHoldingTax);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }

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

        private static void SendPrnInfo(int partnerId, string token, string taxType,  decimal amount, DateTime periodFrom, DateTime periodTo, ILog log)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAApiUrl).StringValue;
            var operatorPin = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAApiUsername).StringValue;
            var transactionDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            var requestInput = new
            {
                hash = CommonFunctions.ComputeSha256($"{operatorPin}{transactionDate}EXCISE"),
                paymentInfo = new
                {
                    PINNO = operatorPin,
                    amount = Math.Round(amount, 0),
                    taxType,
                    transactionDate,
                    periodFrom = periodFrom.ToString("yyyy-MM-dd"),
                    periodTo = periodFrom.ToString("yyyy-MM-dd")
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
            if(!string.IsNullOrEmpty(resp))
                log.Info("generatePrnRequest_resp: " + resp);
        }

        private static decimal SendBetsInfo(int partnerId, List<BetItem> bets, string token, ILog log)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAApiUrl).StringValue;
            var operatorPin = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAApiUsername).StringValue;
            var taxFee = Convert.ToDecimal(CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRATaxFee).StringValue);
            var transactionDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            var requestInput = new
            {
                Request = new
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
                        stakeInfo = new
                        {
                            betId = x.BetId.ToString(),
                            customerId = x.ClientId.ToString(),
                            mobileNo = !string.IsNullOrEmpty(x.MobileNumber) ? x.MobileNumber.Replace("+", string.Empty) : x.ClientId.ToString(),
                            punterAmt = Math.Round(x.BetAmount, 2),
                            stakeAmt = Math.Round(100*x.BetAmount/(taxFee + 100), 2),
                            desc = "Dummy",
                            odds = x.Coefficent,
                            stakeType = x.IsSport ? "NORMAL" : "VIRTUAL",
                            dateOfStake = transactionDate,
                            exciseAmt = Math.Round(x.BetAmount - 100*x.BetAmount/(taxFee + 100), 2),
                            expectedOutcomeTime = x.CalculationDate?.ToString("yyyy-MM-ddTHH:mm:ss") ?? x.PlacementTime.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss"),
                            walletBalanceStake = x.CurrentBalance
                        }
                    }).ToArray()
                }
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
            return !bets.Any() ? 0 : requestInput.Request.details.Sum(x => x.stakeInfo.exciseAmt);
        }

        private static decimal SendWinsInfo(int partnerId, List<BetItem> wins, string token, ILog log)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAApiUrl).StringValue;
            var operatorPin = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAApiUsername).StringValue;
            var taxFee = Convert.ToDecimal(CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRATaxFee).StringValue);
            var withHoldingtaxFee = Convert.ToDecimal(CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.KRAWithHoldingTaxFee).StringValue);
            var transactionDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            var requestInput = new
            {
                Request = new
                {
                    hash = CommonFunctions.ComputeSha256($"{operatorPin}{transactionDate}{wins.Count()}"),
                    header = new
                    {
                        operatorPin,
                        transactionDate,
                        noOfOutcomes = wins.Count()
                    },
                    details = wins.Select(x =>
                    {
                        var stakeAmt = Math.Round(100*x.BetAmount/(taxFee + 100), 2);
                        var r = new
                        {
                            outcomeInfo = new
                            {
                                betId = x.BetId.ToString(),
                                outcome = x.WinAmount > 0 ? "WIN" : "LOSE",
                                outcomedate = x.CalculationDate?.ToString("yyyy-MM-ddTHH:mm:ss"),
                                payout = Math.Round(stakeAmt*x.Coefficent),
                                winnings = Math.Round(stakeAmt*(x.Coefficent-1), 2),
                                withholdingTax = Math.Round(stakeAmt*(x.Coefficent-1)*withHoldingtaxFee/100, 2),
                                walletBalanceOutcome = x.CurrentBalance
                            }
                        }; return r;
                    }).ToArray()
                }
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
            return !wins.Any() ? 0 : requestInput.Request.details.Sum(x => x.outcomeInfo.withholdingTax);
        }
    }
}
