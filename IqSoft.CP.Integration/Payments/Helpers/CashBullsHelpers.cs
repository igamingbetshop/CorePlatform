using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using log4net;
using System.Text;
using IqSoft.CP.Integration.Payments.Models.CashBulls;
using IqSoft.CP.Integration.Payments.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class CashBullsHelpers
    {
        private static Dictionary<string, string> PaymentServices { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.CashBullsBlik, "blik_code"}, //??
            { Constants.PaymentSystems.CashBullsPix, "pix"},
            { Constants.PaymentSystems.CashBullsOpenBanking, "openbanking"}
        };

        public static string CallCashBullsApi(PaymentRequest input, string cashierPageUrl, SessionIdentity sessionIdentity, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            if (!PaymentServices.ContainsKey(paymentSystem.Name))
                throw BaseBll.CreateException(sessionIdentity.LanguageId, Constants.Errors.PaymentSystemNotFound);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.CashBullsApiUrl);
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var paymentInput = new
            {
                amount = Math.Round(input.Amount, 2),
                currency = client.CurrencyId,
                order_id = input.Id.ToString(),
                customer_ip = sessionIdentity.LoginIp,
                redirect_success_url = cashierPageUrl,
                redirect_fail_url = cashierPageUrl,
                pending_url = cashierPageUrl,
                webhook_url = $"{paymentGatewayUrl}/api/CashBulls/ApiRequest",
                payment_method = PaymentServices[paymentSystem.Name]
            };           
            
            var byteArray = Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}:{partnerPaymentSetting.Password}");
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "credentials", Convert.ToBase64String(byteArray) } },
                Url =  $"{url}/api/payment/createForm",
                PostData = JsonConvert.SerializeObject(paymentInput)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(response);

            if (!string.IsNullOrEmpty(paymentOutput.PaymentId))
            {
                using (var paymentSystemBl = new PaymentSystemBll(sessionIdentity, log))
                {
                    input.ExternalTransactionId =  paymentOutput.PaymentId;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
            }
            if (paymentOutput.Status == 1)
                return paymentOutput.RedirectUrl;
            throw new Exception($"Error: {response}");           
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log, out List<int> userIds)
        {
            userIds = new List<int>();
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                               paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CashBullsApiUrl).StringValue;
            var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
            if (!PaymentServices.ContainsKey(paymentSystem.Name))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
            var payoutInput = new
            {
                amount = Math.Round(paymentRequest.Amount, 2),
                currency = client.CurrencyId,
                order_id = paymentRequest.Id.ToString(),
                payment_method = PaymentServices[paymentSystem.Name],
                customer_first_name = client.FirstName,
                customer_last_name = client.LastName,
                webhook_url = $"{paymentGatewayUrl}/api/CashBulls/ApiRequest",
                cpf = paymentInfo.WalletNumber,//PIX
                iban = paymentInfo.Info //openbanking
            };
            var byteArray = Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}:{partnerPaymentSetting.Password}");
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "credentials", Convert.ToBase64String(byteArray) } },
                Url =  $"{url}/api/payout",
                PostData = JsonConvert.SerializeObject(payoutInput, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(response);

            if (!string.IsNullOrEmpty(paymentOutput.PaymentId))
            {
                using (var paymentSystemBl = new PaymentSystemBll(session, log))
                {
                    paymentRequest.ExternalTransactionId =  paymentOutput.PaymentId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                }
            }
            if (paymentOutput.Status != 1)
                throw new Exception($"Error: {response}");
            if (paymentOutput.StatusTransaction.ToLower() == "approved")
            {
                using (var clientBl = new ClientBll(session, log))
                using (var documentBl = new DocumentBll(clientBl))
                using (var notificationBl = new NotificationBll(clientBl))
                {
                    var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, paymentOutput.Description,
                                                                   null, null, false, paymentRequest.Parameters, documentBl, notificationBl, out userIds);
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Approved
                    };
                }
            }
            else
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
        }
    }
}
