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


namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class PraxisHelpers
    {
        public static string CallPraxisApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrWhiteSpace(client.Address))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
            if (string.IsNullOrWhiteSpace(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (client.BirthDate == DateTime.MinValue)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidBirthDate);
            if (string.IsNullOrWhiteSpace(client.ZipCode.Trim()))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
            if (string.IsNullOrEmpty(paymentInfo.City))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, input.Type);
            var url = string.Format("{0}cashier/cashier",
                                    CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PraxisApiUrl).StringValue);
            var applicationKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PraxisApplicationKey)?.StringValue;
            var returnUrl = string.Format("https://{0}/user/1/deposit?get=1", session.Domain);
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;

            var paymentRequestInput = new
            {
                merchant_id = partnerPaymentSetting.UserName,
                application_key = !string.IsNullOrEmpty(applicationKey) ? applicationKey : session.Domain,
                intent = input.Type == (int)PaymentRequestTypes.Deposit ? "payment" : "withdrawal",
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
                requester_ip = session.LoginIp,
                customer_data = new
                {
                    country = paymentInfo.Country,
                    city = paymentInfo.City,
                    zip = client.ZipCode.Trim(),
                    first_name = client.FirstName,
                    last_name = client.LastName,
                    address = client.Address,
                    email = client.Email,
                    dob = client.BirthDate.ToString("MM/DD/YYYY"),
                    state = (int?)null
                }
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
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                RequestHeaders = headers,
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (response.Status != 0)
                throw new Exception(response.Description);
            return response.RedirectUrl;
        }
        /*
        public static string CallPraxisGatewayApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, input.Type);
            var url = string.Format("{0}cashier/cashier",
                                    CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PraxisApiUrl).StringValue);
            var returnUrl = string.Format("https://{0}/user/1/{1}?get=1", session.Domain,
                                                                    input.Type == (int)PaymentRequestTypes.Deposit ? "deposit" : "withdraw");
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var applicationKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PraxisApplicationKey)?.StringValue;

            var merchant = partnerPaymentSetting.UserName.Split(',');
            if (merchant.Length != 2)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerKeyNotFound);
            var paymentRequestInput = new
            {
                merchant_id = merchant[0],
                application_key = !string.IsNullOrEmpty(applicationKey) ? applicationKey : session.Domain,
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
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
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
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                RequestHeaders = headers,
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (response.Status != 0)
                throw new Exception(response.Description);
            return response.RedirectUrl;
        }
        */
        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                            paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                        var url = string.Format("{0}agent/manage-withdrawal-request",
                                                 CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PraxisApiUrl).StringValue);

                        var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                        var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                        var paymentRequestInput = new
                        {
                            merchant_id = partnerPaymentSetting.UserName,
                            application_key = parameters["ApplicationKey"],
                            intent = "complete-withdrawal-request",
                            withdrawal_request_id = paymentRequest.ExternalTransactionId,
                            gateway = paymentInfo.PSPService,
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
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            Url = url,
                            RequestHeaders = headers,
                            PostData = JsonConvert.SerializeObject(paymentRequestInput)
                        };
                        var responseString = CommonFunctions.SendHttpRequest(httpRequestInput, out System.Net.WebHeaderCollection outputHeaders);
                        var response = JsonConvert.DeserializeObject<PayoutOutput>(responseString);
                        if (response.Status != 0)
                        {
                            log.Error($"Input: {JsonConvert.SerializeObject(httpRequestInput)}");
                            throw new Exception(responseString);
                        }
                        var inputSign = outputHeaders.GetValues("GT-Authentication")[0];
                        signature = CommonFunctions.ComputeSha384(response.Status.ToString() + response.Timestamp +
                                                     (response.Transaction?.Tid.ToString() ?? string.Empty) + (response.Transaction?.Status ?? string.Empty) +
                                                     (response.Transaction?.ProcessedCurrency ?? string.Empty)  +
                                                     (response.Transaction?.ProcessedAmount ?? string.Empty)  +
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
        }

        public static void GetPayoutRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            var url = $"{CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PraxisApiUrl).StringValue}agent/find-transaction";
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
            var statusRequestInput = new
            {
                merchant_id = partnerPaymentSetting.UserName,
                application_key = parameters["ApplicationKey"],
                tid = paymentRequest.ExternalTransactionId,
                version = "1.3",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };
            var signature = CommonFunctions.ComputeSha384(statusRequestInput.merchant_id + statusRequestInput.application_key +
                                                          statusRequestInput.timestamp + statusRequestInput.tid +
                                                          partnerPaymentSetting.Password).ToLower();

            var headers = new Dictionary<string, string> { { "GT-Authentication", signature } };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                RequestHeaders = headers,
                PostData = JsonConvert.SerializeObject(statusRequestInput)
            };
            var responseString = CommonFunctions.SendHttpRequest(httpRequestInput, out System.Net.WebHeaderCollection outputHeaders);
            var response = JsonConvert.DeserializeObject<PayoutOutput>(responseString);
            if (response.Status == 0)
            {
                using (var clientBl = new ClientBll(session, log))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            var transactionStatus = response.Transaction.Status.ToLower();
                            if (transactionStatus == "approved")
                            {
                                var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                                                               null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
                                clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                            }
                            else if (transactionStatus == "rejected" || transactionStatus == "cancelled" ||
                                     transactionStatus == "error" || transactionStatus == "chargeback")
                            {
                                clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
                                $"Status: {response.Transaction.Status}, StatusDetails: {response.Transaction.StatusDescription}",
                                null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
                            }
                        }
                    }
                }
            }
        }

        public static void CancelPayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {            
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            using (var clientBl = new ClientBll(paymentSystemBl))
            using (var documentBl = new DocumentBll(clientBl))
            using (var notificationBl = new NotificationBll(paymentSystemBl))
            {
                var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                var status = PaymentRequestStates.Failed;
                if (parameters.ContainsKey("CanceledBy"))
                    status = parameters["CanceledBy"] == ((int)ObjectTypes.Client).ToString() ? PaymentRequestStates.CanceledByClient :
                                                                                                PaymentRequestStates.CanceledByUser;
                if (string.IsNullOrEmpty(paymentRequest.ExternalTransactionId))
                {
                    clientBl.ChangeWithdrawRequestState(paymentRequest.Id, status, status.ToString(), null, null, false,
                                                        paymentRequest.Parameters, documentBl, notificationBl);
                    return;
                }

                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                    paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = string.Format("{0}agent/manage-withdrawal-request",
                                         CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PraxisApiUrl).StringValue);

                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var paymentRequestInput = new
                {
                    merchant_id = partnerPaymentSetting.UserName,
                    application_key = parameters["ApplicationKey"],
                    intent = "cancel-withdrawal-request",
                    withdrawal_request_id = paymentRequest.ExternalTransactionId,
                    gateway = paymentInfo.PSPService,
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
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    RequestHeaders = headers,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var responseString = CommonFunctions.SendHttpRequest(httpRequestInput, out System.Net.WebHeaderCollection outputHeaders);
                log.Error($"input: {JsonConvert.SerializeObject(httpRequestInput)}, responseString: {responseString}");
                var response = JsonConvert.DeserializeObject<PayoutOutput>(responseString);
                if (response.Status != 0)
                {
                    log.Error($"Input: {JsonConvert.SerializeObject(httpRequestInput)}");
                    throw new Exception(responseString);
                }
                var inputSign = outputHeaders.GetValues("GT-Authentication")[0];
                signature = CommonFunctions.ComputeSha384(response.Status.ToString() + response.Timestamp +
                                             (response.Transaction?.Tid.ToString() ?? string.Empty) + (response.Transaction?.Status ?? string.Empty) +
                                             (response.Transaction?.ProcessedCurrency ?? string.Empty)  +
                                             (response.Transaction?.ProcessedAmount ?? string.Empty)  +
                                             partnerPaymentSetting.Password).ToLower();
                if (inputSign.ToLower() != signature.ToLower() || response.Status != 0)
                {
                    log.Error(string.Format("WrongHash_ Input: {0}  Header: {1}", responseString, inputSign));
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                }
                if (response.Status == 0 && response.Transaction.Status.ToLower() == "cancelled")
                {
                   
                    clientBl.ChangeWithdrawRequestState(paymentRequest.Id, status, status.ToString(), null, null, false,
                                                        paymentRequest.Parameters, documentBl, notificationBl);
                    return;
                }

                throw new Exception(responseString);
            }
        }
    }
}