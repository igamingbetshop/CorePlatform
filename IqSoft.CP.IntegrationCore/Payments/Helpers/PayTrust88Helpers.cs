using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.PayTrust88;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class PayTrust88Helpers
    {
        public static string CallPayTrust88Api(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            if (string.IsNullOrWhiteSpace(input.Info))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);

            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var requestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(partnerPaymentSetting.Password + ":")) } };
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayTrust88Url).StringValue;
                var requestInput = new PaymentRequestInput
                {
                    return_url = cashierPageUrl,
                    failed_return_url = cashierPageUrl,
                    http_post_url = string.Format("{0}/api/PayTrust88/ApiRequest", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue),
                    amount = input.Amount,
                    currency = client.CurrencyId,
                    item_id = input.Id.ToString(),
                    item_description = input.Id.ToString(),
                    name = client.Id.ToString(),
                    email = client.Email
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpMethod.Post,
                    Url = string.Format("{0}/transaction/start?{1}", url, CommonFunctions.GetUriEndocingFromObject(requestInput)),
                    PostData = string.Empty,
                    RequestHeaders = requestHeaders
                };
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var output = serializer.Deserialize<PaymentRequestOutput>(resp);
                return output.redirect_to;
            }
        }

        public static PaymentResponse CreatePayment(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBll = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var requestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(partnerPaymentSetting.Password + ":")) } };
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var url = string.Format("{0}/{1}", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayTrust88Url).StringValue,
                                        "payout/start");
                var bankInfo = paymentSystemBll.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                if (bankInfo == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                var requestInput = new PayoutRequestInput
                {
                    amount = amount,
                    currency = client.CurrencyId,
                    name = paymentInfo.BankAccountHolder,
                    bank_name = bankInfo.BankName,
                    bank_code = bankInfo.BankCode,
                    iban = paymentInfo.BankAccountNumber,
                    http_post_url = string.Format("{0}/api/PayTrust88/PayoutResult", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue),
                    item_id = input.Id.ToString(),
                    item_description = input.Id.ToString()
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpMethod.Post,
                    Url = url,
                    PostData = CommonFunctions.GetUriEndocingFromObject(requestInput),
                    RequestHeaders = requestHeaders
                };
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var output = serializer.Deserialize<PayoutRequestOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                input.ExternalTransactionId = output.ExternalTransactionId.ToString();
                paymentSystemBll.ChangePaymentRequestDetails(input);
                var response = new PaymentResponse
                {
                    Status = output.Status >= 0 ? PaymentRequestStates.PayPanding : PaymentRequestStates.Failed
                };
                // response.Description = output.Description;
                return response;
            }
        }

        public static void GetBankCodes(int partnerId)
        {
            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayTrust88);
            if (paymentSystem == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);

            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, paymentSystem.Id, "MYR", (int)PaymentRequestTypes.Withdraw);
            if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
            var partner = CacheManager.GetPartnerById(partnerId);
            if (partner == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerNotFound);
            Dictionary<string, string> requestHeaders = new Dictionary<string, string>();

            requestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes("nMPCb7BEGSWGbMsm08C3hz0HC8woGOkU:")));
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Get,
                Url = "https://api.paytrust88.com/v1/bank",
                RequestHeaders = requestHeaders
            };

            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
    }
}