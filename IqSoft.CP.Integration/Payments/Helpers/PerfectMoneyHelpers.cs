using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.PerfectMoney;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class PerfectMoneyHelpers
    {
        private static Dictionary<string, string> PaymentWays { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.PerfectMoneyWallet, "account" },
            { Constants.PaymentSystems.PerfectMoneyVoucher, "voucher" },
            { Constants.PaymentSystems.PerfectMoneyMobile, "sms" },
            { Constants.PaymentSystems.PerfectMoneyWire, "wire" }
        };
        public static string CallPerfectMoneyApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBll = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBll))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                    var segment = clientBl.GetClientPaymentSegments(input.ClientId.Value, partnerPaymentSetting.PaymentSystemId).OrderBy(x => x.Priority).FirstOrDefault();

                    var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                    if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                        distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                    var link = segment == null ? distributionUrlKey.StringValue : segment.ApiUrl;
                    var url = string.Format(link, session.Domain);
                    var uName = segment == null ? partnerPaymentSetting.UserName : segment.ApiKey.Split('/')[0];
                    var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                    var amount = input.Amount;
                    if (input.CurrencyId != Constants.Currencies.USADollar)
                    {
                        var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.USADollar, partnerPaymentSetting);
                        amount = rate * input.Amount;
                        var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                        parameters.Add("Currency", Constants.Currencies.USADollar);
                        parameters.Add("AppliedRate", rate.ToString("F"));
                        input.Parameters = JsonConvert.SerializeObject(parameters);
                        paymentSystemBll.ChangePaymentRequestDetails(input);
                    }
                    var paymentRequestInput = new PaymentRequestInput
                    {
                        MerchantId = uName,
                        MerchantName = partner.Name,
                        Amount = Math.Floor(amount * 100) / 100,
                        CurrencyId = Constants.Currencies.USADollar,
                        PaymentRequestId = input.Id.ToString(),
                        StatusUrl = string.Format("{0}/{1}", paymentGateway,"api/PerfectMoney/ApiRequest"),
                        PaymentUrl = cashierPageUrl,
                        PaymentMethod = PaymentWays[paymentSystem.Name],
                        Language = CommonHelpers.LanguageISOCodes[session.LanguageId]
                    };
                    var properties = from p in paymentRequestInput.GetType().GetProperties()
                                     select (p.GetValue(paymentRequestInput, null) != null ? p.GetValue(paymentRequestInput, null).ToString() : string.Empty);

                    paymentRequestInput.InputData = CommonFunctions.ComputeMd5(CommonFunctions.ComputeMd5(string.Join(":", properties.ToArray().
                                                                 Where(x => !string.IsNullOrEmpty(x) && x[x.Length - 1] != '='))));
                    return string.Format("{0}/PerfectMoney/PaymentRequest?{1}", url, CommonFunctions.GetUriEndocingFromObject(paymentRequestInput));
                }
            }
        }

        public static PaymentResponse PayVoucher(PaymentRequest input, SessionIdentity session, ILog log, out List<int> userIds)
        {
            userIds = new List<int>();
            using (var clientBl = new ClientBll(session, log))
            {
                using (var notificationBl = new NotificationBll(clientBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                    var segment = clientBl.GetClientPaymentSegments(input.ClientId.Value, partnerPaymentSetting.PaymentSystemId).OrderBy(x => x.Priority).FirstOrDefault();

                    var payerAccount = segment == null ? partnerPaymentSetting.UserName.Split('/') : segment.ApiKey.Split('/');
                    if ((segment == null && payerAccount.Length != 2) || (segment != null && payerAccount.Length != 3))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ImpermissiblePaymentSetting);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);

                    var inp = new
                    {
                        AccountID = payerAccount[0],
                        PassPhrase = segment == null ? partnerPaymentSetting.Password : payerAccount[2],
                        Payee_Account = payerAccount[1],
                        ev_number = paymentInfo.VoucherNumber,
                        ev_code = paymentInfo.ActivationCode
                    };
                    var result = ParsePerfectMoneyResponse(CommonFunctions.SendHttpRequest(new HttpRequestInput
                    {
                        Url = "https://perfectmoney.is/acct/ev_activate.asp",
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        PostData = CommonFunctions.GetUriDataFromObject(inp)
                    }, out _));


                    if (result.Keys.Contains("ERROR"))
                    {
                        clientBl.ChangeDepositRequestState(input.Id, PaymentRequestStates.Failed, string.Empty, notificationBl);
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.Failed,
                            Description = result["ERROR"]
                        };
                    }
                    input.ExternalTransactionId = result["PAYMENT_BATCH_NUM"];
                    var amount = Convert.ToDecimal(result["VOUCHER_AMOUNT"]);
                    if (input.CurrencyId != Constants.Currencies.USADollar)
                    {
                        var rate = BaseBll.GetPaymentCurrenciesDifference(Constants.Currencies.USADollar, client.CurrencyId, partnerPaymentSetting);
                        amount *= rate;
                        var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                        parameters.Add("Currency", Constants.Currencies.USADollar);
                        parameters.Add("AppliedRate", rate.ToString("F"));
                        input.Parameters = JsonConvert.SerializeObject(parameters);
                    }
                    input.Amount = Math.Floor(amount * 100) / 100;
                    using (var paymentSystemBl = new PaymentSystemBll(clientBl))
                    {

                        paymentSystemBl.ChangePaymentRequestDetails(input);
                        clientBl.ApproveDepositFromPaymentSystem(input, false, out userIds);
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.Approved,
                        };
                    }
                }
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                        input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var segment = clientBl.GetClientPaymentSegments(input.ClientId.Value, partnerPaymentSetting.PaymentSystemId).OrderBy(x => x.Priority).FirstOrDefault();
                    if (partnerPaymentSetting == null || (segment == null && string.IsNullOrEmpty(partnerPaymentSetting.UserName)))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                    var payerAccount = segment==null ? partnerPaymentSetting.UserName.Split('/') : segment.ApiKey.Split('/');
                    if ((segment == null && payerAccount.Length != 2) || (segment != null && payerAccount.Length != 3))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ImpermissiblePaymentSetting);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    if (partner == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerNotFound);
                    var url = segment == null ? CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.PerfectMoneyPayoutApiUrl).StringValue : segment.ApiKey;
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var amount = input.Amount - (input.CommissionAmount ?? 0);
                    if (input.CurrencyId != Constants.Currencies.USADollar)
                    {
                        var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.USADollar, partnerPaymentSetting);
                        amount *= rate;
                        var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                        parameters.Add("Currency", Constants.Currencies.USADollar);
                        parameters.Add("AppliedRate", rate.ToString("F"));
                        input.Parameters = JsonConvert.SerializeObject(parameters);
                    }

                    var payoutRequestInput = new
                    {
                        AccountID = payerAccount[0],
                        PassPhrase = segment == null ? partnerPaymentSetting.Password : payerAccount[2],
                        Payer_Account = payerAccount[1],
                        Payee_Account = paymentInfo.WalletNumber,
                        Amount = Math.Floor(amount * 100) / 100,
                        Memo = partner.Name,
                        PAYMENT_ID = input.Id
                    };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = url,
                        PostData = CommonFunctions.GetUriDataFromObject(payoutRequestInput)
                    };
                    var result = ParsePerfectMoneyResponse(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    if (!result.Keys.Contains("ERROR"))
                    {
                        input.ExternalTransactionId = result["PAYMENT_BATCH_NUM"];
                        paymentInfo.BatchNumber = result["PAYMENT_BATCH_NUM"];
                        paymentInfo.PayerAccount = payerAccount[1];
                        paymentInfo.PayeeAccount = paymentInfo.WalletNumber;

                        paymentSystemBl.ChangePaymentRequestDetails(input);
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.Approved,
                        };
                    }
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Failed,
                        Description = result["ERROR"]
                    };
                }
            }
        }

        public static PaymentResponse CreateVoucher(PaymentRequest input, SessionIdentity session, ILog log, out List<int> userIds)
        {
            userIds = new List<int>();
            using (var clientBl = new ClientBll(session, log))
            {
                using (var notificationBl = new NotificationBll(clientBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var segment = clientBl.GetClientPaymentSegments(input.ClientId.Value, partnerPaymentSetting.PaymentSystemId).OrderBy(x => x.Priority).FirstOrDefault();

                    var payerAccount = segment == null ? partnerPaymentSetting.UserName.Split('/') : segment.ApiKey.Split('/');
                    if ((segment == null && payerAccount.Length != 2) || (segment != null && payerAccount.Length != 3))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ImpermissiblePaymentSetting);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var amount = input.Amount - (input.CommissionAmount ?? 0);
                    if (input.CurrencyId != Constants.Currencies.USADollar)
                    {
                        var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.USADollar, partnerPaymentSetting);
                        amount *= rate;
                        var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                        parameters.Add("Currency", Constants.Currencies.USADollar);
                        parameters.Add("AppliedRate", rate.ToString("F"));
                        input.Parameters = JsonConvert.SerializeObject(parameters);
                    }
                    var inp = new
                    {
                        AccountID = payerAccount[0],
                        PassPhrase = segment == null ? partnerPaymentSetting.Password : payerAccount[2],
                        Payer_Account = payerAccount[1],
                        Amount = Convert.ToInt32(amount)
                    };
                    var result = ParsePerfectMoneyResponse(CommonFunctions.SendHttpRequest(new HttpRequestInput
                    {
                        Url = "https://perfectmoney.is/acct/ev_create.asp",
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        PostData = CommonFunctions.GetUriDataFromObject(inp)
                    }, out _));

                    if (result.Keys.Contains("ERROR"))
                    {
                        clientBl.ChangeDepositRequestState(input.Id, PaymentRequestStates.Failed, result["ERROR"], notificationBl);
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.Failed,
                            Description = result["ERROR"]
                        };
                    }
                    input.ExternalTransactionId = result["PAYMENT_BATCH_NUM"];
                    using (var paymentSystemBl = new PaymentSystemBll(clientBl))
                    {
                        using (var documentBl = new DocumentBll(clientBl))
                        {
                            input.Info = JsonConvert.SerializeObject(
                            new PaymentInfo
                            {
                                VoucherNum = result["VOUCHER_NUM"],
                                VoucherCode = result["VOUCHER_CODE"],
                                VoucherAmount = result["VOUCHER_AMOUNT"],
                                BatchNumber = result["PAYMENT_BATCH_NUM"]
                            },
                            new JsonSerializerSettings()
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Ignore
                            });
                            paymentSystemBl.ChangePaymentRequestDetails(input);
                            clientBl.ChangeWithdrawRequestState(input.Id, PaymentRequestStates.PayPanding, string.Empty,
                                                                null, null, true, input.Parameters, documentBl, notificationBl, out userIds, false);
                            var resp = clientBl.ChangeWithdrawRequestState(input.Id, PaymentRequestStates.Approved, string.Empty,
                                                                           null, null, false, string.Empty, documentBl, notificationBl, out userIds);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                            return new PaymentResponse
                            {
                                Status = PaymentRequestStates.Approved
                            };
                        }
                    }
                }
            }
        }

        private static Dictionary<string, string> ParsePerfectMoneyResponse(string source)
        {
            if (source == null) return null;

            var regEx = new Regex("<input name='(.*)' type='hidden' value='(.*)'>");
            var matches = regEx.Matches(source);
            var results = new Dictionary<string, string>();
            foreach (Match match in matches)
            {
                results.Add(match.Groups[1].Value, match.Groups[2].Value);
            }
            return results;
        }
    }
}