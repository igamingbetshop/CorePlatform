using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using System;
using IqSoft.CP.Integration.Payments.Models.PaymentAsia;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Integration.Payments.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class PaymentAsiaHelpers
    {
        public static string CallPaymentAsiaApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentAsiaApiUrl).StringValue;
                var paymentRequestInput = new PaymentInput
                {
                    merchant_reference = input.Id.ToString(),
                    amount = input.Amount.ToString("F"),
                    return_url= cashierPageUrl,
                    currency = client.CurrencyId,
                    customer_email = client.Email,
                    customer_ip = session.LoginIp,
                    customer_first_name = client.FirstName,
                    customer_last_name = client.LastName,
                    customer_phone = client.MobileNumber,
                    network = "DirectDebit"
                };
                var hash = CommonFunctions.GetSortedParamWithValuesAsString(paymentRequestInput, "&");
                paymentRequestInput.sign = CommonFunctions.ComputeSha512(hash + partnerPaymentSetting.Password).ToLower();
                var returnUrl = string.Format(CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl).StringValue, session.Domain);

                var apiUrl = AESEncryptHelper.EncryptDistributionString(string.Format("{0}/{1}", url, partnerPaymentSetting.UserName));
                var requestBody = string.Format("{0}&apiUrl={1}", CommonFunctions.GetUriEndocingFromObject(paymentRequestInput), apiUrl);
                requestBody = AESEncryptHelper.EncryptDistributionString(requestBody);
                return string.Format("{0}/paymentasia/PaymentRequest?p={1}", returnUrl, requestBody);
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                    paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentAsiaPayoutApiUrl).StringValue;
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                if (bankInfo == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);

                var paymentGtw = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var payoutRequestInput = new PayoutInput
                {
                    request_reference = paymentRequest.Id.ToString(),
                    beneficiary_name = paymentInfo.BankAccountHolder,
                    beneficiary_first_name = client.FirstName,
                    beneficiary_last_name = client.LastName,
                    bank_name = bankInfo.BankCode,
                    beneficiary_email = client.Email,
                    beneficiary_phone = client.MobileNumber,
                    account_number = paymentInfo.BankAccountNumber,
                    amount = amount.ToString("F"),
                    currency = client.CurrencyId,
                    datafeed_url = string.Format("{0}/api/PaymentAsia/PayoutRequest", paymentGtw)
                };

                var hash = CommonFunctions.GetSortedParamWithValuesAsString(payoutRequestInput, "&").Replace("%20", "+");
                payoutRequestInput.sign = CommonFunctions.ComputeSha512(hash + partnerPaymentSetting.Password).ToLower();
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpMethod.Post,
                    Url = string.Format(url, partnerPaymentSetting.UserName),
                    PostData = CommonFunctions.GetUriEndocingFromObject(payoutRequestInput).Replace("%20", "+")
                };
                var payoutRequestOutput = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));

                paymentRequest.ExternalTransactionId = payoutRequestOutput.request.id;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                if (payoutRequestOutput.response.code!= 200)
                    throw new Exception(string.Format("Code: {0}, Error: {1}", payoutRequestOutput.response.code, payoutRequestOutput.response.message));

                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
        }
    }
}