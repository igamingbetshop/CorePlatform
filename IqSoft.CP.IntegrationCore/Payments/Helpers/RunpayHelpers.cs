using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using IqSoft.CP.Integration.Payments.Models.Runpay;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class RunpayHelpers
    {
        private static readonly string CertificatePath = @"C:\Certificates\RunPay\client_{0}.pfx";

        public static string CallRunpayApi(PaymentRequest input)
        {
            var client = CacheManager.GetClientById(input.ClientId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.RunpayApiUrl).StringValue;
            var paymentRequestInput = new
            {
                LMI_MERCHANT_ID = partnerPaymentSetting.UserName,
                LMI_PAYMENT_AMOUNT = input.Amount,
                LMI_CURRENCY = client.CurrencyId,
                LMI_PAYMENT_NO = input.Id,
                LMI_PAYMENT_DESC = partner.Name
            };
            return string.Format("{0}payment/init?{1}", url, CommonFunctions.GetUriEndocingFromObject(paymentRequestInput));
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                    paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.RunpayPayoutApiUrl).StringValue;
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var names = partnerPaymentSetting.UserName.Split('|');
                var keys = partnerPaymentSetting.Password.Split('|');
                var currentTime = DateTime.UtcNow.ToString("yyymmddhhmmss");
                var payoutRequestInput = JsonConvert.SerializeObject(new
                {
                    clientTranId = paymentRequest.Id.ToString(),
                    account = paymentInfo.MobileNumber,
                    amount,
                    commissionAmount = 0m,
                    currency = client.CurrencyId,
                    operatorCode = Convert.ToInt32(names[0])
                });
                var timestamp = (Int64)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                var certificate = new X509Certificate2(string.Format(CertificatePath, client.PartnerId.ToString()), keys[1]);
                var requestHeaders = new Dictionary<string, string>
                {
                    { "RP-CLIENT", names[1] } ,
                    { "RP-TS", timestamp.ToString() } ,
                    { "RP-SIGN", CommonFunctions.ComputeHMACSha256($"{names[1]}{timestamp}{payoutRequestInput}", keys[0]) }
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    Url = string.Format("{0}/init", url),
                    RequestHeaders = requestHeaders,
                    PostData = payoutRequestInput
                };

                var resp = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _,
                    certificate: certificate, ignoreCertificate: true));

                paymentRequest.ExternalTransactionId = resp.ServerTranId.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                if (resp.ErrorCode != 0)
                    throw new Exception(string.Format("Code: {0}, Error: {1}", resp.ErrorCode, resp.ErrorMessage));

                var confirmInput = JsonConvert.SerializeObject(new
                {
                    serverTranId = resp.ServerTranId,
                    account = resp.Account,
                    amount = resp.Amount,
                    commissionAmount = 0,
                    currency = client.CurrencyId,
                    operatorCode = names[0]
                });
                timestamp = (Int64)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                requestHeaders = new Dictionary<string, string>
                {
                    { "RP-CLIENT", names[1] } ,
                    { "RP-TS", timestamp.ToString() } ,
                    { "RP-SIGN", CommonFunctions.ComputeHMACSha256($"{names[1]}{timestamp}{confirmInput}", keys[0]) }
                };
                httpRequestInput.Url = string.Format("{0}/confirm", url);
                httpRequestInput.RequestHeaders = requestHeaders;
                httpRequestInput.PostData = confirmInput;

                resp = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _,
                                                                   certificate: certificate, ignoreCertificate: true));
                if (resp.Status.ToLower() == "paysuccess" && resp.ErrorCode == 0)
                {
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Approved,
                    };
                }
                throw new Exception(string.Format("Code: {0}, Error: {1}", resp.ErrorCode, resp.ErrorMessage));
            }
        }
    }
}