using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Integration.Payments.Models.WalletOne;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Integration.Payments.Models;
using System.Net;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class WalletOneProvider
    {
        public const string Card = "k367";
        public const string Wallet = "WalletOneKZT";
    }

    public static class WalletOneHelpers
    {
        public static string CallWalletOneApi(PaymentRequest input, int partnerId, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            if (string.IsNullOrWhiteSpace(input.Info))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
            string response;
            using var currencyBll = new CurrencyBll(session, log);
            using var paymentSystemBl = new PaymentSystemBll(currencyBll);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var merchantId = partnerPaymentSetting.UserName;
            var secretKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.WalletOneSecretKey).StringValue;
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.WalletOneDepositUrl).StringValue;   
            var language = (session.LanguageId != null &&
                            session.LanguageId.ToLower() == Constants.Languages.English)
                ? "en-US"
                : "ru-RU";
            string currencyCode = currencyBll.GetCurrencyById(input.CurrencyId).Code;
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            var paymentRequestInput = new WalletOneInput
            {
                WMI_MERCHANT_ID = merchantId,
                WMI_RECIPIENT_LOGIN = paymentInfo.Info,
                WMI_PAYMENT_AMOUNT = input.Amount.ToString("F"),
                WMI_CURRENCY_ID = currencyCode,
                WMI_PAYMENT_NO = input.Id,
                WMI_DESCRIPTION = string.Empty,
                WMI_SUCCESS_URL = cashierPageUrl,
                WMI_FAIL_URL = cashierPageUrl,
                WMI_CULTURE_ID = language,
                WMI_AUTO_LOCATION = 1,
                WMI_EXPIRED_DATE = DateTime.UtcNow.AddHours(1).ToString("s", System.Globalization.CultureInfo.InvariantCulture),
                WMI_SIGNATURE = string.Empty
            };

            Byte[] bytes = Encoding.GetEncoding("windows-1251").GetBytes(CommonFunctions.GetSortedValuesAsString(paymentRequestInput) + secretKey);
            using (var md5 = MD5.Create())
            {
                Byte[] hash = md5.ComputeHash(bytes);

                paymentRequestInput.WMI_SIGNATURE = Convert.ToBase64String(hash);

                var requestData = new RequestInput { Content = CommonFunctions.GetUriEndocingFromObject<WalletOneInput>(paymentRequestInput) };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    Url = url,
                    PostData = JsonConvert.SerializeObject(requestData)
                };

                var serializer = new JavaScriptSerializer();
                response = serializer.Deserialize<string>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                input.ExternalTransactionId = response.Substring(response.IndexOf("=") + 1);
                paymentSystemBl.ChangePaymentRequestDetails(input);
            }
            return response;
        }
             
        public static PaymentResponse SendPaymentRequestToCreditCards(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
            if (Regex.IsMatch(paymentInfo.CardNumber, @"^d{4}|d{4}|d{4}|d{4}$"))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
            var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);

            var inputInfo = new WalletOneFormInput
            {
                Params = new WalletOneFormBaseFields[] {
                                                         new WalletOneFormBaseFields { FieldId = "CardNumber", Value = paymentInfo.CardNumber.Replace(" ","")},
                                                         new WalletOneFormBaseFields { FieldId = "Amount", Value = amount.ToString("0.##") }
                                                       }
            };
            paymentRequest.Info = JsonConvert.SerializeObject(inputInfo);
            return CreatePayment(paymentRequest, WalletOneProvider.Card, session, log);
        }

        public static PaymentResponse SendPaymentRequestToWalletOne(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
            if (string.IsNullOrWhiteSpace(paymentInfo.Info))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
            var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
            var inputInfo = new WalletOneFormInput
            {
                Params = new WalletOneFormBaseFields[] {
                                                         new WalletOneFormBaseFields { FieldId = "account", Value = !string.IsNullOrEmpty(paymentInfo.Info ) ? paymentInfo.Info : paymentInfo.WalletNumber },
                                                         new WalletOneFormBaseFields { FieldId = "Amount", Value = amount.ToString("0.##") }
                                                       }
            };

            paymentRequest.Info = JsonConvert.SerializeObject(inputInfo);
            return CreatePayment(paymentRequest, WalletOneProvider.Wallet, session, log);
        }

        public static PaymentResponse CreatePayment(PaymentRequest paymentRequest, string provider, SessionIdentity session, ILog log)
        {
            var resState = new PaymentResponse { Status = PaymentRequestStates.Failed };
            try
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId, paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);

                using var partnerBl = new PartnerBll(session, log);
                using var paymentSystemBl = new PaymentSystemBll(session, log);
                var token = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.WalletOneToken);
                var secretKey = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.WalletOneWithdrawKey);
                var url = partnerBl.GetPaymentValueByKey(null, paymentRequest.PaymentSystemId, Constants.PartnerKeys.WalletOneWithdrawUrl);
                var paymentInput = new WalletOnePaymentInput
                {
                    ProviderId = provider,
                    ExternalId = paymentRequest.Id.ToString()
                };
                var serializer = new JavaScriptSerializer();
                var responseText = SendRequest(url, token, secretKey, JsonConvert.SerializeObject(paymentInput), HttpMethod.Post, string.Empty, log);
                var response = serializer.Deserialize<WalletOneFormOutput>(responseText);

                var formFilledValues = serializer.Deserialize<WalletOneFormInput>(paymentRequest.Info);
                formFilledValues.FormId = response.Form.FormId;
                paymentRequest.Info = serializer.Serialize(formFilledValues);
                var w1PaymentId = response.PaymentId;

                paymentRequest.ExternalTransactionId = w1PaymentId;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                var newUrl = url + "?externalId=" + paymentInput.ExternalId;
                responseText = SendRequest(newUrl, token, secretKey, JsonConvert.SerializeObject(formFilledValues),
                                           HttpMethod.Put, paymentInput.ExternalId, log);

                response = serializer.Deserialize<WalletOneFormOutput>(responseText);
                var finalInput = new WalletOneFinalInput { FormId = response.Form.FormId };
                responseText = SendRequest(newUrl, token, secretKey, JsonConvert.SerializeObject(finalInput),
                                           HttpMethod.Put, paymentInput.ExternalId, log);

                var finalOutput = serializer.Deserialize<WalletOneFinalOutput>(responseText);
                resState.Status = W1StateConvertToPaymentRequestStates(finalOutput.State.StateId);
            }
            catch (Exception exc)
            {
                resState.Description = exc.Message;
                log.Error(exc.Message);
            }
            return resState;
        }
        public static void GetPayoutRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using var partnerBl = new PartnerBll(session, log);
            using var clientBl = new ClientBll(partnerBl);
            using var notificationBl = new NotificationBll(partnerBl);
            var client = CacheManager.GetClientById(paymentRequest.ClientId);
            var token = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.WalletOneToken);
            var secretKey = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.WalletOneWithdrawKey);

            var url = string.Format("{0}?externalId={1}",
                CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.WalletOneWithdrawUrl), paymentRequest.ExternalTransactionId);
            var serializer = new JavaScriptSerializer();
            var formFilledValues = serializer.Deserialize<WalletOneFormInput>(paymentRequest.Info);
            var finalInput = new WalletOneFinalInput { FormId = formFilledValues.FormId };
            var responseText = SendRequest(url, token, secretKey, JsonConvert.SerializeObject(finalInput),
                                           HttpMethod.Put, paymentRequest.ExternalTransactionId, log);

            var finalOutput = serializer.Deserialize<WalletOneFinalOutput>(responseText);
            var status = W1StateConvertToPaymentRequestStates(finalOutput.State.StateId);
            if (status == PaymentRequestStates.Approved)
            {
                using var documentBl = new DocumentBll(clientBl);
                var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty, null, null, 
                                                              false, string.Empty, documentBl, notificationBl);
                clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
            }
            else if (status == PaymentRequestStates.Failed)
            {
                using var documentBl = new DocumentBll(clientBl);
                clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, finalOutput.State.Description,
                                                    null, null, false, string.Empty, documentBl, notificationBl);
            }            
        }

        private static string SendRequest(string url, string token, string key, string postData, HttpMethod method, string w1PaymentId, ILog log)
        {
            var contentType = "application/vnd.wallet.openapi.v1+json";
            var timestamp = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
           var requestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + token },
                                                                 { "X-Wallet-Timestamp", timestamp } };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = contentType,
                RequestMethod = method,
                Url = url,
                PostData = postData,
                Accept = contentType
            };

            var sign = "https://api.w1.ru/OpenApi/payments";
            sign += !string.IsNullOrEmpty(w1PaymentId) ? "?externalId=" + w1PaymentId : string.Empty;
            sign += token + timestamp + httpRequestInput.PostData + key;
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sign));
                requestHeaders.Add("X-Wallet-Signature", Convert.ToBase64String(hash));
            }
            httpRequestInput.RequestHeaders = requestHeaders;
            log.Info(JsonConvert.SerializeObject(httpRequestInput));
            return CommonFunctions.SendHttpRequest(httpRequestInput, out _/*, SecurityProtocolType.Ssl3*/); //old
        }

        private static PaymentRequestStates W1StateConvertToPaymentRequestStates(string w1State)
        {
            return w1State switch
            {
                "Created"or"Updated"or"Checked"or"Processing"or"Checking" => PaymentRequestStates.PayPanding,
                "Blocked"or"ProcessError"or"CheckError"or"PayError"or"Canceled" => PaymentRequestStates.Failed,
                "Paid" => PaymentRequestStates.Approved,
                _ => PaymentRequestStates.Failed,
            };
        }
    }
}
