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

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class NOWPayHelpers
    {
        public static string CallNOWPayApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NOWPayApiUrl).StringValue;
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var resourcesUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResourcesUrl);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);

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
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = headers,
                Url = string.Format("{0}{1}", url, "payment"),
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var paymentOutput = new PaymentOutput();
            try
            {
                var req = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(req);
            }
            catch(Exception ex) // error message contains api key which is visible  for player
            {
                var errorOutput = JsonConvert.DeserializeObject<ErrorOutput>(ex.Message);
                var message = JsonConvert.DeserializeObject<ErrorOutput>(errorOutput.Message);
                throw new Exception(message.Message);
            }          

            if (paymentOutput.Payment_status?.ToLower() == "failed")
                throw new Exception(paymentOutput.Payment_status);

            var paymentProcessingInput = new
            {
                PayAddress = paymentOutput.Pay_address,
                Amount = paymentOutput.Pay_amount,
                Currency = paymentOutput.Pay_currency,
                PartnerDomain = session.Domain,
                ResourcesUrl = (resourcesUrlKey == null || resourcesUrlKey.Id == 0 ? ("https://resources." + session.Domain) : resourcesUrlKey.StringValue),
                PartnerId = client.PartnerId,
                LanguageId = session.LanguageId,
                PaymentSystemName = input.PaymentSystemName.ToLower()
            };

            var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
            if(distributionUrlKey == null || distributionUrlKey.Id == 0)
                distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

            var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
            var data = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(paymentProcessingInput));
            return string.Format("{0}/paymentform/paymentprocessing?data={1}", distributionUrl, data);
        }

        public static string GetTransactionWalletNumber(long paymentRequestId, int clientId, decimal amount, string cryptoCurrency, ILog log)
        {
            var client = CacheManager.GetClientById(clientId);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NOWPayApiUrl).StringValue;
            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.NOWPay);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,paymentSystem.Id,
                                                                               client.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var paymentRequestInput = new
            {
                price_amount = Math.Round(amount, 2),
                price_currency = client.CurrencyId,
                pay_currency = cryptoCurrency?.ToLower(),
                ipn_callback_url = string.Format("{0}/api/NOWPay/ApiRequest", paymentGatewayUrl),
                order_id = paymentRequestId
            };

            var headers = new Dictionary<string, string>
                        {
                            {"x-api-key",partnerPaymentSetting.Password }
                        };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = headers,
                Url = string.Format("{0}{1}", url, "payment"),
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            log.Info("CallNOWPayApi_" + response);
            var resp = JsonConvert.DeserializeObject<PaymentOutput>(response);
            if (resp.Payment_status?.ToLower() == "failed")
                throw new Exception(resp.Payment_status);
            return resp.Pay_address;
        }

        public static string CallNOWPayFiatApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NOWPayApiUrl).StringValue;
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;

            var paymentRequestInput = new
            {
                price_amount = Math.Round(input.Amount, 2),
                price_currency = client.CurrencyId,
                pay_currency = client.CurrencyId,
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
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = headers,
                Url = string.Format("{0}{1}", url, "payment"),
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var req = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            log.Info("CallNOWPayApi_" + req);
            var resp = JsonConvert.DeserializeObject<PaymentFiatOutput>(req);
            return resp.RedirectData.RedirectUrl;
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
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
                    RequestMethod = Constants.HttpRequestMethods.Post,
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
                    RequestMethod = Constants.HttpRequestMethods.Post,
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
