using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Transact365;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class Transact365Helpers
    {
        private static readonly string ApiVersion = "2.0";
        public static string PaymentRequest(PaymentRequest paymentRequest, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(paymentRequest.ClientId);
                    if (string.IsNullOrEmpty(client.MobileNumber) || string.IsNullOrEmpty(client.Email))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
                    if (string.IsNullOrEmpty(client.Address))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
                    if (string.IsNullOrEmpty(client.ZipCode?.Trim()))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");
                    if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);

                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                       client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                    var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.Transact365Url);
                    var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                    var amount = Math.Round(paymentRequest.Amount, 2);
                    if (client.CurrencyId != Constants.Currencies.JapaneseYen)
                    {
                        var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.JapaneseYen, partnerPaymentSetting);
                        amount = Math.Round(rate * paymentRequest.Amount, 2);
                        var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                        parameters.Add("Currency", Constants.Currencies.JapaneseYen);
                        parameters.Add("AppliedRate", rate.ToString("F"));
                        parameters.Add("Amount", amount.ToString());
                        paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    }
                    var input = new
                    {
                        amount = amount * 100,
                        auth_key = partnerPaymentSetting.UserName,
                        auth_timestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        auth_version = ApiVersion,
                        callback_url = string.Format("{0}/api/Transact365/ApiRequest", paymentGateway),
                        city = paymentInfo.City,
                        country = session.Country,
                        currency = Constants.Currencies.JapaneseYen,
                        email = client.Email,
                        name = client.Id,
                        order_id = paymentRequest.Id,
                        phone = client.MobileNumber.Replace("+", string.Empty),
                        postal_code = client.ZipCode.Trim(),
                        return_url = cashierPageUrl,
                        street = client.Address,
                    };
                    log.Info(JsonConvert.SerializeObject(input));
                    var data = CommonFunctions.GetUriDataFromObject(input);
                    log.Info(JsonConvert.SerializeObject(data));
                    var signature = CommonFunctions.ComputeHMACSha256($"POST\npdapi\n{data}", partnerPaymentSetting.Password).ToLower();
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = HttpMethod.Post,
                        Url = url + "deposit/p2p",
                        PostData = $"auth_signature={signature}&{data}"
                    };
                    log.Info(JsonConvert.SerializeObject(httpRequestInput.PostData));
                    var output = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    if (output.StatusCode == 3002)
                    {
                        paymentRequest.ExternalTransactionId = output.TransId;
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                        return output.Url;
                    }
                    throw new Exception($"Error: {output.StatusMessage}");
                }
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(paymentRequest.ClientId);
                    if (string.IsNullOrEmpty(client.MobileNumber) || string.IsNullOrEmpty(client.Email))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
                    if (string.IsNullOrEmpty(client.Address))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
                    if (string.IsNullOrEmpty(client.ZipCode?.Trim()))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");
                    if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                       client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.Transact365Url);
                    var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                    var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                    amount = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.JapaneseYen, amount) * 100;
                    var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                    if (bankInfo == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                    var holderNameInput = Regex.Replace(paymentInfo.NationalId, @"\s+", " ");
                    var holderNames = holderNameInput.Split(' ');
                    if(holderNames.Length != 2)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
                    var input = new
                    {
                        account_no = paymentInfo.BankAccountNumber,
                        amount = Convert.ToInt32(amount),
                        auth_key = partnerPaymentSetting.UserName,
                        auth_timestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        auth_version = ApiVersion,
                        bank = bankInfo.BankCode,
                        branch = paymentInfo.BankBranchName,
                        callback_url = string.Format("{0}/api/Transact365/ApiRequest", paymentGateway),
                        city = client.Address,
                        country = paymentInfo.Country,
                        currency = Constants.Currencies.JapaneseYen,
                        email = client.Email,
                        id_number = client.Id,
                        last_name = holderNames[1],
                        name = holderNames[0],
                        order_id = paymentRequest.Id,
                        phone = client.MobileNumber.Replace("+", ""),
                        postal_code = client.ZipCode.Trim(),
                        state = client.State.ToString(),
                        street = client.Address
                    };
                    var data = CommonFunctions.GetUriDataFromObject(input);
                    var hash = string.Format("{0}{1}", "POST\npdapi\n", data);
                    var signature = CommonFunctions.ComputeHMACSha256(hash, partnerPaymentSetting.Password).ToLower();
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = HttpMethod.Post,
                        Url = url + "payout",
                        PostData = $"auth_signature={signature}&" + data
                    };
                    var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
                    if (output.StatusCode == 3002)
                    {
                        paymentRequest.ExternalTransactionId = output.TransId;
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding,
                        };
                    }
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Failed,
                        Description = output.StatusMessage
                    };
                }
            }
        }
    }
}
