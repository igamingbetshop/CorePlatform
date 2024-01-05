using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.NodaPay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class NodaPayHelpers
    {
        public static string CallNodaPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.NodaPayApiUrl);
            var amount = input.Amount;
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
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
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;

                var paymentRequestInput = new
                {
                    amount,
                    currency = Constants.Currencies.Euro,
                    customerId = client.Id.ToString(),
                    description = partner.Name,
                    shopId = partnerPaymentSetting.UserName,
                    paymentId = input.Id.ToString(),
                    returnUrl = cashierPageUrl,
                    webhookUrl = string.Format("{0}/api/NodaPay/ApiRequest", paymentGateway)
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/api/payments", url),
                    RequestHeaders = new Dictionary<string, string> { { "x-api-key", partnerPaymentSetting.Password.Split(',')[0] } },
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                log.Debug(JsonConvert.SerializeObject(httpRequestInput));
                var paymentRequestOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (!string.IsNullOrEmpty(paymentRequestOutput.Id))
                {
                    input.ExternalTransactionId = paymentRequestOutput.Id;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                //if (!string.IsNullOrEmpty(paymentRequestOutput.ErrorCode))
                //    throw new Exception(string.Format("Code: {0}, Error: {1}", paymentRequestOutput.ErrorCode, paymentRequestOutput.ErrorDescription));

                return paymentRequestOutput.Url;
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);

                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.NodaPayApiUrl);
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
                    iban = paymentInfo.WalletNumber, // template 20
                    beneficiaryName = paymentInfo.AccountType,
                    webhookUrl = string.Format("{0}/api/NodaPay/PayoutRequest", paymentGateway),
                    requestId = paymentRequest.Id.ToString()
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/api/payouts", url),
                    RequestHeaders = new Dictionary<string, string>{  { "x-api-key", partnerPaymentSetting.UserName } },
                    PostData = JsonConvert.SerializeObject(payoutRequestInput)
                };
                var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var payoutResponse = JsonConvert.DeserializeObject<PayoutResponse>(response);
                if(payoutResponse.Status.ToLower() == "failed")
                    return new PaymentResponse
                    {
                        Description = response,
                        Status = PaymentRequestStates.Failed,
                    };
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
        }     
    }
}
