using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Praxis;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class PraxisHelpers
    {
        public static string CallPraxisApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = string.Format("{0}cashier/cashier",
                                    CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PraxisApiUrl).StringValue);
            var returnUrl = string.Format("https://{0}/user/1/deposit?get=1", session.Domain);
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;

            var paymentRequestInput = new
            {
                merchant_id = partnerPaymentSetting.UserName,
                application_key = session.Domain,
                intent = "payment",
                currency = input.CurrencyId,
                amount = Convert.ToInt32(input.Amount * 100),
                cid = input.ClientId.ToString(),
                locale = CommonHelpers.LanguageISOCodes[session.LanguageId],
                notification_url = string.Format("{0}/api/Praxis/{1}", paymentGatewayUrl,
                                                 input.Type == (int)PaymentRequestTypes.Deposit ? "ApiRequest" : "Authentication"),
                return_url = returnUrl,
                order_id = input.Id.ToString(),
                version = "1.3",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                parameters.Add("ApplicationKey", paymentRequestInput.application_key);
                input.Parameters = JsonConvert.SerializeObject(parameters);
                paymentSystemBl.ChangePaymentRequestDetails(input);
            }
            var signature = CommonFunctions.ComputeSha384(paymentRequestInput.merchant_id + paymentRequestInput.application_key +
                                                          paymentRequestInput.timestamp + paymentRequestInput.intent +
                                                          paymentRequestInput.cid + paymentRequestInput.order_id +
                                                          partnerPaymentSetting.Password).ToLower();
            var headers = new Dictionary<string, string> { { "GT-Authentication", signature } };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = url,
                RequestHeaders = headers,
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (response.Status != 0)
                throw new Exception(response.Description);
            return response.RedirectUrl;
        }

        public static string CallPraxisGatewayApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = string.Format("{0}cashier/cashier",
                                    CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PraxisApiUrl).StringValue);
            var returnUrl = string.Format("https://{0}/user/1/{1}?get=1", session.Domain,
                                                                    input.Type == (int)PaymentRequestTypes.Deposit ? "deposit" : "withdraw");
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var merchant = partnerPaymentSetting.UserName.Split(',');
            if (merchant.Length != 2)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerKeyNotFound);
            var paymentRequestInput = new
            {
                merchant_id = merchant[0],
                application_key = session.Domain,
                intent = input.Type == (int)PaymentRequestTypes.Deposit ? "payment" : "withdrawal",
                currency = input.CurrencyId,
                gateway = merchant[1],
                amount = Convert.ToInt32(input.Amount * 100),
                cid = input.ClientId.ToString(),
                locale = CommonHelpers.LanguageISOCodes[session.LanguageId],
                notification_url = string.Format("{0}/api/Praxis/{1}", paymentGatewayUrl,
                                                 input.Type == (int)PaymentRequestTypes.Deposit ? "ApiRequest" : "Authentication"),
                return_url = returnUrl,
                order_id = input.Id.ToString(),
                version = "1.3",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                parameters.Add("ApplicationKey", paymentRequestInput.application_key);
                input.Parameters = JsonConvert.SerializeObject(parameters);
                paymentSystemBl.ChangePaymentRequestDetails(input);
            }
            var signature = CommonFunctions.ComputeSha384(paymentRequestInput.merchant_id + paymentRequestInput.application_key +
                                                          paymentRequestInput.timestamp + paymentRequestInput.intent +
                                                          paymentRequestInput.cid + paymentRequestInput.order_id +
                                                          partnerPaymentSetting.Password).ToLower();
            var headers = new Dictionary<string, string> { { "GT-Authentication", signature } };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = url,
                RequestHeaders = headers,
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (response.Status != 0)
                throw new Exception(response.Description);
            return response.RedirectUrl;
        }
        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using var paymentSystemBl = new PaymentSystemBll(session, log);
            using var clientBl = new ClientBll(paymentSystemBl);
            using var documentBl = new DocumentBll(paymentSystemBl);
            var client = CacheManager.GetClientById(paymentRequest.ClientId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            var url = string.Format("{0}agent/manage-withdrawal-request",
                                     CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PraxisApiUrl).StringValue);
            var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
            var merchant = partnerPaymentSetting.UserName.Split(',');
            if (merchant.Length != 2)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerKeyNotFound);
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
            var paymentRequestInput = new
            {
                merchant_id = merchant[0],
                application_key = parameters["ApplicationKey"],
                intent = "complete-withdrawal-request",
                withdrawal_request_id = paymentRequest.ExternalTransactionId,
                gateway = merchant[1],
                version = "1.3",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };
            var signature = CommonFunctions.ComputeSha384(paymentRequestInput.merchant_id + paymentRequestInput.application_key +
                                                          paymentRequestInput.timestamp + paymentRequestInput.intent +
                                                          paymentRequestInput.withdrawal_request_id +
                                                          partnerPaymentSetting.Password).ToLower();
            var headers = new Dictionary<string, string> { { "GT-Authentication", signature } };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = url,
                RequestHeaders = headers,
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var responseString = CommonFunctions.SendHttpRequest(httpRequestInput, out HttpResponseHeaders outputHeaders);
            var response = JsonConvert.DeserializeObject<PayoutOutput>(responseString);

            var inputSign = outputHeaders.GetValues("GT-Authentication").First();
            signature = CommonFunctions.ComputeSha384(response.Status.ToString() + response.Timestamp +
                                         response.Transaction.Tid + response.Transaction.Status +
                                         (response.Transaction?.ProcessedCurrency ?? string.Empty)  +
                                         (response.Transaction.ProcessedAmount ?? string.Empty)  +
                                         partnerPaymentSetting.Password).ToLower();
            if (inputSign.ToLower() != signature.ToLower())
            {
                log.Error(string.Format("WrongHash_ Input: {0}  Header: {1}", responseString, inputSign));
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            }
            if (response.Status == 0)
            {
                paymentRequest.ExternalTransactionId = response.Transaction.Tid.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                var transactionStatus = response.Transaction.Status.ToLower();
                using (var notificationBl = new NotificationBll(clientBl))
                {
                    if (transactionStatus == "approved")
                    {
                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, transactionStatus,
                                                                       null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.Approved
                        };
                    }
                    else if (transactionStatus == "rejected" || transactionStatus == "cancelled" ||
                             transactionStatus == "error" || transactionStatus == "chargeback")
                    {
                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, transactionStatus,
                                                             null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
                        return new PaymentResponse
                        {
                            Description = responseString,
                            Status = PaymentRequestStates.Failed
                        };
                    }
                    else
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding
                        };
                }
            }
            return new PaymentResponse
            {
                Description = responseString,
                Status = PaymentRequestStates.Failed
            };
        }
    }
}