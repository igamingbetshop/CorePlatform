// Author: Varsik Harutyunyan
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.NOWPay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class NOWPayHelpers
    {
        public static string CallNOWPayApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NOWPayApiUrl).StringValue;
            var returnUrl = string.Format("https://{0}/user/1/deposit", session.Domain);
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            var resourcesUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResourcesUrl);

            var paymentRequestInput = new
            {
                price_amount = Math.Round(input.Amount, 2),
                price_currency = client.CurrencyId,
                pay_currency = paymentInfo.AccountType?.ToLower(),
                ipn_callback_url = string.Format("{0}/api/NOWPay/ApiRequest", paymentGatewayUrl),
                order_id = input.Id,
            };

            var headers = new Dictionary<string, string>
                        {
                            {"x-api-key",partnerPaymentSetting.Password }
                        };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = HttpMethod.Post,
                RequestHeaders = headers,
                Url = string.Format("{0}{1}", url, "payment"),
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var req = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            log.Info("CallNOWPayApi_" + req);
            var resp = JsonConvert.DeserializeObject<PaymentOutput>(req);
            if (resp.Payment_status?.ToLower() == "failed")
                throw new Exception(resp.Payment_status);

            var paymentProcessingInput = new
            {
                PayAddress = resp.Pay_address,
                Amount = resp.Pay_amount,
                PayCurrency = resp.Pay_currency,
                PartnerDomain = session.Domain,
                ResourcesUrl = (resourcesUrlKey == null || resourcesUrlKey.Id == 0 ? ("https://resources." + session.Domain) : resourcesUrlKey.StringValue),
                PartnerId = client.PartnerId,
                LanguageId = session.LanguageId,
                PaymentSystemName = input.PaymentSystemName.ToLower()
            };

            var distributionUrl = string.Format(CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl).StringValue, session.Domain);
            var data = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(paymentProcessingInput));
            return string.Format("{0}/paymentform/paymentprocessing?data={1}", distributionUrl, data);
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                    paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NOWPayApiUrl).StringValue;
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;

                var headers = new Dictionary<string, string>
                        {
                            {"x-api-key",partnerPaymentSetting.Password }
                        };
                var info = partnerPaymentSetting.UserName.Split(',');
                var body = new
                {
                    email = info.FirstOrDefault(),
                    password = info.LastOrDefault(),
                };
                var jsonBody = JsonConvert.SerializeObject(body);
                var authRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    RequestHeaders = headers,
                    Url = string.Format("{0}{1}", url, "auth"),
                    PostData = jsonBody
                };
                var authResp = CommonFunctions.SendHttpRequest(authRequestInput, out _);
                var resp = JsonConvert.DeserializeObject<AuthModel>(authResp);
                var token = resp.Token;

                var withdrawal = new
                {
                    address = paymentInfo.WalletNumber,
                    currency = paymentInfo.AccountType?.ToLower(),
                    amount,
                    ipn_callback_url = string.Format("{0}/api/NOWPay/PayoutRequest", paymentGateway),
                    fiat_amount = amount,
                    fiat_currency = client.CurrencyId
                };
                List<object> withdrawals = new List<object>
                {
                    withdrawal
                };
                var payoutRequestInput = new
                {
                    ipn_callback_url = string.Format("{0}/api/NOWPay/PayoutRequest", paymentGateway),
                    withdrawals = withdrawals
                };
                headers.Add("Authorization", "Bearer " + token);
                log.Info("RequestBody_" + JsonConvert.SerializeObject(payoutRequestInput));
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    Url = string.Format("{0}{1}", url, "payout"),
                    RequestHeaders = headers,
                    PostData = JsonConvert.SerializeObject(payoutRequestInput)
                };
                log.Info("Response_" + authResp);
                var req = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var paymentRequestOutput = JsonConvert.DeserializeObject<PayoutOutput>(req);
                if (paymentRequestOutput.Withdrawals.Any())
                {
                    var infon = paymentRequestOutput.Withdrawals.FirstOrDefault();
                    paymentRequest.ExternalTransactionId = paymentRequestOutput.Id;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                    if (infon.Status.ToLower() == "finished")
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.Approved,
                        };
                    if (infon.Status.ToLower() == "failed")
                        throw new Exception(string.Format("Code: {0}, Error: {1}", infon.Status, infon.Error));
                    else
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding,
                        };
                }
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Failed,
                };
            }
        }
    }
}