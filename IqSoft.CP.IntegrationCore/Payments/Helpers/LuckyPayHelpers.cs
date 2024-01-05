using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.LuckyPay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class LuckyPayHelpers
    {
        public static PaymentResponse SendWithdrawRequestToProvider(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId, paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                if (string.IsNullOrEmpty(partnerPaymentSetting.UserName) || string.IsNullOrEmpty(partnerPaymentSetting.Password))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerProductSettingNotFound);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var loginInput = new
                {
                    username = partnerPaymentSetting.UserName,
                    password = partnerPaymentSetting.Password
                };
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.LuckyPayWithdrawUrl).StringValue;
                var loginOutput = CallToApi(loginInput, url + "persona/authenticate", string.Empty);
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var prepareInput = new
                {
                    client = new { usercode = paymentInfo.MobileNumber },
                    amount,
                    currency = paymentRequest.CurrencyId,
                    transactionId = paymentRequest.Id
                };
                log.Info("Acceptinput_" + JsonConvert.SerializeObject(prepareInput));
                var prepareOutput = CallToApi(prepareInput, url + "transaction/withdraw", JsonConvert.DeserializeObject<LoginOutput>(loginOutput).jwt);
                log.Info("AcceptOutput_" + JsonConvert.SerializeObject(prepareOutput));
                var output = JsonConvert.DeserializeObject<WithdrawOutput>(prepareOutput);
                paymentRequest.ExternalTransactionId = output.transactionId;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Approved
                };
            }
        }

        public static string SendDepositRequestToProvider(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBll = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.LuckyPayDepositUrl).StringValue;
                var backUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue +
                        string.Format("{0}?id={1}&url={2}", "/LuckyPay/NotifyResult", input.Id, cashierPageUrl);
                var paymentRequestInput = new
                {
                    MerchantId = partnerPaymentSetting.UserName,
                    Amount = input.Amount,
                    CallbackUrl = backUrl,
                    OrderId = input.Id,
                    OrderDescription = client.UserName
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    Url = url,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var response = JsonConvert.DeserializeObject<DepositOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.status == 1)
                {
                    input.ExternalTransactionId = response.transactionId;
                    paymentSystemBll.ChangePaymentRequestDetails(input);
                    return string.Format("{0}/{1}", url, response.transactionId);
                }
                throw new Exception(response.message);
            }
        }

        private static string CallToApi(object input, string url, string jwt)
        {
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = url,
                PostData = JsonConvert.SerializeObject(input),
                RequestHeaders = new Dictionary<string, string>()
            };
            if (!string.IsNullOrEmpty(jwt))
                httpRequestInput.RequestHeaders.Add("Authorization", "Bearer " + jwt);
            return CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
    }
}
    