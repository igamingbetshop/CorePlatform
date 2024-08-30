using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models.Nixxe;
using IqSoft.CP.Integration.Payments.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class NixxeHelpers
    {
        public static string CallNixxeApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (string.IsNullOrEmpty(client.MobileNumber))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);

            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, input.Type);
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.PaymentGateway).StringValue;
            var callbackUrl = string.Format("{0}/api/Nixxe/ApiRequest", paymentGatewayUrl);
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.NixxeApiUrl);
            var mode = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.NixxeApiMode);
            if (string.IsNullOrEmpty(mode) || (mode!="test"  && mode!= "prod"))
                mode = "prod";
            var paymentInput = new
            {
                request = new
                {
                    mode,
                    settings = new
                    {
                        status_url = callbackUrl,
                        return_url = cashierPageUrl
                    },
                    user_id = client.Id.ToString(),
                    tracking_id = input.Id.ToString(),
                    ip = session.LoginIp,
                    amount = input.Amount,
                    amount_restricted = true,
                    currency = client.CurrencyId,
                    customer = new
                    {
                        first_name = client.FirstName,
                        last_name = client.LastName,
                        email = client.Email,
                        zip_code = "dummy",
                        address = "dummy",
                        city = "dummy",
                        kyc_status = "passed",
                        dob = client.BirthDate.ToString("dd/MM/yyyy"),
                        country_code = session.Country,
                        phone = new
                        {
                            country = session.Country,
                            number = client.MobileNumber.Replace(client.PhoneNumber, string.Empty),
                            prefix = client.PhoneNumber
                        }
                    }
                }
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "x-api-key", partnerPaymentSetting.UserName } },
                Url = input.Type == (int)PaymentRequestTypes.Withdraw ? $"{url}/v1/withdrawal/checkout/get_token" :
                                                                        $"{url}/v1/payment/checkout/get_token",
                PostData = JsonConvert.SerializeObject(paymentInput)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
            if (!string.IsNullOrEmpty(output.RedirectUrl))
                return output.RedirectUrl;
            throw new Exception($"Error: {response}");

        }

        public static PaymentResponse ApprovePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                               paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.NixxeApiUrl);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "x-api-key", partnerPaymentSetting.UserName } },
                Url =  $"{url}/v1/withdrawal/approve",
                PostData = JsonConvert.SerializeObject(new { transaction_id = paymentRequest.Id.ToString() })
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var payoutOutput = JsonConvert.DeserializeObject<PayoutOutput>(response);
            if (payoutOutput.Code != 0)
                throw new Exception(response);
            return new PaymentResponse
            {
                Status = PaymentRequestStates.PayPanding,
            };
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

                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.NixxeApiUrl);
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "x-api-key", partnerPaymentSetting.UserName } },
                    Url =  $"{url}/v1/withdrawal/reject",
                    PostData = JsonConvert.SerializeObject(new { transaction_id = paymentRequest.ExternalTransactionId })
                };
                var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var payoutOutput = JsonConvert.DeserializeObject<PayoutOutput>(response);
                if (payoutOutput.Code != 0)
                    throw new Exception(response);

                if (payoutOutput.TransactionStatus.ToLower() != "successful")
                    status = PaymentRequestStates.CancelPending;
                clientBl.ChangeWithdrawRequestState(paymentRequest.Id, status, payoutOutput.TransactionStatus, null, null, false,
                                                    paymentRequest.Parameters, documentBl, notificationBl);
            }
        }
    }
}
