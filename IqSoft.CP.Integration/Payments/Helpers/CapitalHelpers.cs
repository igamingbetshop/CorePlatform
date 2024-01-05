using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using IqSoft.CP.Integration.Payments.Models.Capital;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Integration.Payments.Models;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class CapitalHelpers
    {
        public static string CallCapitalApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CapitalApiUrl).StringValue;
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
                var paymentRequestInput = new
                {
                    merchant_id = partnerPaymentSetting.UserName.Split(',')[1],
                    merchant_token = partnerPaymentSetting.Password,
                    merchant_site_id = partnerPaymentSetting.UserName.Split(',')[0],
                    payment_method = paymentSystem.Name == Constants.PaymentSystems.Sofort ? "sofort" : "crypto",
                    rules = "VIP",
                    device_ip = session.LoginIp,
                    amount = paymentSystem.Name == Constants.PaymentSystems.Sofort ? amount : 0,
                    currency = paymentSystem.Name == Constants.PaymentSystems.Capital ? paymentInfo.Info : client.CurrencyId,
                    email = client.Email,
                    language = session.LanguageId,
                    request_id = input.Id,
                    country = "NL",//country.IsoCode,
                    first_name = client.Id,
                    last_name = client.UserName,
                    notification_link = string.Format("{0}/api/Capital/ApiRequest", paymentGateway),
                    success_url = cashierPageUrl,
                    pending_url = cashierPageUrl,
                    fail_url = cashierPageUrl,
                    back_url = cashierPageUrl,
                    mobile = client.MobileNumber
                };
                var headers = new Dictionary<string, string> { { "merchant_token", partnerPaymentSetting.Password } };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = headers,
                    Url = string.Format("{0}payment", url),
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var paymentRequestOutput = JsonConvert.DeserializeObject<PaymentOutput>(resp);
                if (!string.IsNullOrEmpty(paymentRequestOutput.ExternalTransactionId))
                {
                    input.ExternalTransactionId = paymentRequestOutput.ExternalTransactionId;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                if (paymentRequestOutput.Status != "SUCCESS")
                    throw new Exception(string.Format("Code: {0}, Error: {1}", paymentRequestOutput.Status, paymentRequestOutput.Status));
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
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CapitalApiUrl).StringValue;
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");

                var paymentRequestInput = new
                {
                    merchant_id = partnerPaymentSetting.UserName.Split(',')[1],
                    merchant_token = partnerPaymentSetting.Password,
                    merchant_site_id = partnerPaymentSetting.UserName.Split(',')[0],
                    payment_method = paymentSystem.Name == Constants.PaymentSystems.Sofort ? "sofort" : "crypto",
                    device_ip = session.LoginIp,
                    amount = paymentSystem.Name == Constants.PaymentSystems.Sofort ? amount : 0,
                    currency = client.CurrencyId,
                    email = client.Email,
                    language = session.LanguageId,
                    request_id = paymentRequest.Id,
                    country = paymentInfo.Country,
                    first_name = client.Id,
                    last_name = client.UserName,
                    notification_link = string.Format("{0}/api/Capital/ApiRequest", paymentGateway),
                    //success_url = returnUrl,
                    //pending_url = returnUrl,
                    //fail_url = returnUrl,
                    //back_url = returnUrl,
                    mobile = client.MobileNumber,
                    crypto_address = paymentInfo.WalletNumber
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}payment", url),
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var paymentRequestOutput = JsonConvert.DeserializeObject<PaymentOutput>(resp);
                if (!string.IsNullOrEmpty(paymentRequestOutput.ExternalTransactionId))
                {
                    paymentRequest.ExternalTransactionId = paymentRequestOutput.ExternalTransactionId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                }
                if (paymentRequestOutput.Status != "SUCCESS")
                    throw new Exception(string.Format("Code: {0}, Error: {1}", paymentRequestOutput.Status, paymentRequestOutput.Status));

                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
        }
    }
}
