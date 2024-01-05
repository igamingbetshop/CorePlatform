using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Astropay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class AstropayHelpers
    {
        public static string CallAstroPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.AstropayApiUrl).StringValue;
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                if (input.CurrencyId != Constants.Currencies.Euro)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.Euro);
                    amount = Math.Round(rate * input.Amount, 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.Euro);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                var paymentRequestInput = new
                {
                    amount = amount.ToString(),
                    currency = Constants.Currencies.Euro,
                    country = paymentInfo.Country,
                    merchant_deposit_id = input.Id.ToString(),
                    callback_url = string.Format("{0}/api/Astropay/ApiRequest", paymentGateway),
                    redirect_url = cashierPageUrl,
                    user = new
                    {
                        merchant_user_id = client.Id
                    },
                    product = new
                    {
                        mcc = 7995,
                        merchant_code = "casino",
                        description = partner.Name
                    }
                };
                var postData = JsonConvert.SerializeObject(paymentRequestInput);
                var headers = new Dictionary<string, string>
                {
                    { "Merchant-Gateway-Api-Key", partnerPaymentSetting.Password },
                    { "Signature", CommonFunctions.ComputeHMACSha256(postData, partnerPaymentSetting.UserName).ToLower() }
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/merchant/v1/deposit/init", url),
                    RequestHeaders = headers,
                    PostData = postData
                };
                log.Info(postData);
                var paymentRequestOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (!string.IsNullOrEmpty(paymentRequestOutput.ExternalId))
                {
                    input.ExternalTransactionId = paymentRequestOutput.ExternalId;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                if (!string.IsNullOrEmpty(paymentRequestOutput.ErrorCode))
                    throw new Exception(string.Format("Code: {0}, Error: {1}", paymentRequestOutput.ErrorCode, paymentRequestOutput.ErrorDescription));
                return paymentRequestOutput.RedirectUrl;
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                    paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.AstropayApiUrl).StringValue;

                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                if (paymentRequest.CurrencyId != Constants.Currencies.Euro)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.Euro);
                    amount = Math.Round(rate * paymentRequest.Amount, 2);
                    var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                    parameters.Add("Currency", Constants.Currencies.Euro);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                }
                var payoutRequestInput = new
                {
                    amount,
                    currency = Constants.Currencies.Euro,
                    country = paymentInfo.Country,
                    merchant_cashout_id = paymentRequest.Id.ToString(),
                    callback_url = string.Format("{0}/api/Astropay/PayoutRequest", paymentGateway),
                    user = new
                    {
                        phone = paymentInfo.WalletNumber,
                        merchant_user_id = client.Id
                    }
                };
                var postData = JsonConvert.SerializeObject(payoutRequestInput);
                var headers = new Dictionary<string, string>
                    {
                        { "Merchant-Gateway-Api-Key", partnerPaymentSetting.Password },
                        { "Signature", CommonFunctions.ComputeHMACSha256(postData, partnerPaymentSetting.UserName).ToLower() }
                    };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/merchant/v1/cashout", url),
                    RequestHeaders = headers,
                    PostData = postData
                };
                var paymentRequestOutput = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (!string.IsNullOrEmpty(paymentRequestOutput.ExternalId))
                {
                    paymentRequest.ExternalTransactionId = paymentRequestOutput.ExternalId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                }
                if (!string.IsNullOrEmpty(paymentRequestOutput.ErrorCode))
                    throw new Exception(string.Format("Code: {0}, Error: {1}", paymentRequestOutput.ErrorCode, paymentRequestOutput.ErrorDescription));

                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
        }



        public static void GetPayoutRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var notificationBl = new NotificationBll(partnerBl))
                {
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                        paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.AstropayApiUrl).StringValue;
                    var headers = new Dictionary<string, string>
                    {
                        { "Merchant-Gateway-Api-Key", partnerPaymentSetting.Password },
                        { "Signature", CommonFunctions.ComputeHMACSha256("", partnerPaymentSetting.UserName).ToLower() }
                    };

                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Get,
                        Url = string.Format("{0}/merchant/v1/cashout/{1}/status", url, "120677"),
                        RequestHeaders = headers
                    };
                    var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                }
            }
        }
    }
}
