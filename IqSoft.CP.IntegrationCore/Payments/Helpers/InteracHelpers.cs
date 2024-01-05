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
using IqSoft.CP.Integration.Payments.Models.Interac;
using System.Collections.Generic;
using System.Text;
using System;
using IqSoft.CP.Integration.Payments.Models;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class InteracHelpers
    {
        public static string CallInteracApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId);
            if (client.CurrencyId != Constants.Currencies.CanadianDollar)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                if (string.IsNullOrEmpty(client.MobileNumber) || string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.InteracApiUrl);
                var sandbox = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.InteracSandbox);
                if (string.IsNullOrEmpty(url))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerKeyNotFound);
                var paymentInput = new
                {
                    userId = client.Id.ToString(),
                    site = cashierPageUrl,
                    userIp = session.LoginIp,
                    currency = client.CurrencyId,
                    amount = input.Amount,
                    transactionId = input.Id,
                    type = "CPI",
                    name = client.UserName,
                    email = client.Email,
                    mobile = client.MobileNumber.Replace("+", string.Empty),
                    sandbox = !string.IsNullOrEmpty(sandbox) ? sandbox : "false"
                };
                var byteArray = Encoding.Default.GetBytes(partnerPaymentSetting.Password);
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpMethod.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(byteArray) } },
                    Url = string.Format("{0}/api/payment-token/{1}", url, partnerPaymentSetting.UserName),
                    PostData = CommonFunctions.GetUriDataFromObject(paymentInput)
                };
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (!string.IsNullOrEmpty(paymentOutput.Error))
                    throw new Exception(paymentOutput.Error);
                input.ExternalTransactionId =  paymentOutput.Data.TransactionId;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return string.Format("{0}/webflow?transaction={1}&token={2}", url, paymentOutput.Data.TransactionId, paymentOutput.Token);
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.InteracApiUrl);
                var sandbox = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.InteracSandbox);
                if (string.IsNullOrEmpty(url))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerKeyNotFound);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var paymentInput = new
                {
                    userId = client.Id.ToString(),
                    site = "https://" + paymentInfo.Domain,
                    userIp = paymentInfo.TransactionIp,
                    currency = client.CurrencyId,
                    amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0),
                    transactionId = paymentRequest.Id,
                    type = "ACH",
                    hosted = false,
                    name = client.UserName,
                    fi = paymentInfo.BankCode, // 3 digits
                    transit = paymentInfo.BankBranchName, //5 digits
                    acct = paymentInfo.BankAccountNumber, // 7 - 12 digit
                    sandbox = !string.IsNullOrEmpty(sandbox) ? sandbox : "false"
                };
                var byteArray = Encoding.Default.GetBytes(partnerPaymentSetting.Password);
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpMethod.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(byteArray) } },
                    Url = string.Format("{0}/api/payment-token/{1}", url, partnerPaymentSetting.UserName),
                    PostData = CommonFunctions.GetUriDataFromObject(paymentInput)
                };
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (!string.IsNullOrEmpty(paymentOutput.Error))
                    throw new Exception(paymentOutput.Error);
                paymentRequest.ExternalTransactionId =  paymentOutput.Data.TransactionId;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                httpRequestInput.Url =  string.Format("{0}/webflow?transaction={1}", url, paymentOutput.Data.TransactionId);
                httpRequestInput.RequestMethod = HttpMethod.Post;
                httpRequestInput.PostData = "token=" +paymentOutput.Token;
                var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                httpRequestInput.RequestMethod = HttpMethod.Get;
                httpRequestInput.Url =  string.Format("{0}/webflow/deposit?transaction={1}&token={2}", url, paymentOutput.Data.TransactionId, paymentOutput.Token);
                httpRequestInput.PostData = string.Empty;
                var payoutResult = JsonConvert.DeserializeObject<PayoutResult>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (payoutResult.Status == "STATUS_INITED")
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                    };
                throw new Exception(payoutResult.Status);
            }

        }
    }
}
