using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.EasyPay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class EasyPayHelpers
    {
        public static string CallEasyPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.EasyPayUrl).StringValue;
                //var returnUrl = string.Format("https://{0}/user/2/account", session.Domain);              

                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var paymentRequestInput = new
                {
                    operationType = "debit",
                    merchantId = partnerPaymentSetting.UserName,
                    customerId = client.Id,
                    amount = input.Amount,
                    description = partner.Name,
                    country = !string.IsNullOrEmpty(session.Country) ? session.Country : "ES",
                    currency = client.CurrencyId,
                    firstname = client.Id,
                    lastname = client.UserName,
                    merchantTransactionId = input.Id,
                    language = session.LanguageId,
                    successURL = cashierPageUrl,
                    errorURL = string.Format("https://{0}", session.Domain),
                    cancelURL = cashierPageUrl,
                    statusURL = string.Format("{0}/{1}", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue, "api/EasyPay/ApiRequest"),

                };
                var data = CommonFunctions.GetUriDataFromObject(paymentRequestInput);
                var encrypted = AesEncryption(data, partnerPaymentSetting.Password);
                string integrityCheck = sha256_hash(data);

                var encryptedInput = new
                {
                    encrypted = System.Net.WebUtility.UrlEncode(encrypted),
                    integrityCheck,
                    merchantId = partnerPaymentSetting.UserName
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/tokenize?{1}", url, CommonFunctions.GetUriDataFromObject(encryptedInput))
                };
                return CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBll = new ClientBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                        input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                    var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.EasyPayUrl).StringValue;
                    var returnUrl = string.Format("https://{0}/user/2/account", session.Domain);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    if (string.IsNullOrEmpty(paymentInfo.CardNumber))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                    var paymentCardInfo = clientBll.GetClientPaymentAccountDetails(client.Id, input.PaymentSystemId, new List<int> { (int)ClientPaymentInfoTypes.CreditCard }, false)
                                           .FirstOrDefault(x => x.CardNumber == paymentInfo.CardNumber);
                    if (paymentCardInfo == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);
                    var payoutRequestInput = new
                    {
                        operationType = "credit",
                        merchantId = partnerPaymentSetting.UserName,
                        customerId = client.Id,
                        cardNumberToken = paymentCardInfo.WalletNumber,
                        paymentSolution = "creditcards",
                        amount = input.Amount - (input.CommissionAmount ?? 0),
                        country = !string.IsNullOrEmpty(session.Country) ? session.Country : "ES",
                        currency = client.CurrencyId,
                        merchantTransactionId = input.Id,
                        statusURL = string.Format("{0}/{1}", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue, "api/EasyPay/PayoutRequest"),
                    };
                    var data = CommonFunctions.GetUriDataFromObject(payoutRequestInput);
                    var encrypted = AesEncryption(data, partnerPaymentSetting.Password);
                    string integrityCheck = sha256_hash(data);

                    var encryptedInput = new
                    {
                        encrypted = System.Net.WebUtility.UrlEncode(encrypted),
                        integrityCheck,
                        merchantId = partnerPaymentSetting.UserName
                    };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = string.Format("{0}/pay?{1}", url, CommonFunctions.GetUriDataFromObject(encryptedInput))
                    };
                    var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    var serializer = new XmlSerializer(typeof(payfrexresponse), new XmlRootAttribute("payfrex-response"));
                    var output = (payfrexresponse)serializer.Deserialize(new StringReader(res));
                    if (output.status.ToUpper() != "ERROR")
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding,
                            Description = output.message
                        };
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Failed,
                        Description = output.message
                    };
                }
            }
        }

        //public static void GetPayoutRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        //{
        //    using (var partnerBl = new PartnerBll(session, log))
        //    {
        //        using (var notificationBl = new NotificationBll(partnerBl))
        //        {
        //            var client = CacheManager.GetClientById(paymentRequest.ClientId);
        //            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
        //                paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
        //            var partner = CacheManager.GetPartnerById(client.PartnerId);
        //            var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.EasyPayUrl).StringValue;
        //            var payoutStatusRequestInput = new
        //            {
        //                merchantId = partnerPaymentSetting.UserName,
        //                token = CommonFunctions.ComputeMd5( string.Format("{0}.{1}.{2}", partnerPaymentSetting.UserName,350, partnerPaymentSetting.Password)),
        //                transactions = 350
        //            };
        //            var httpRequestInput = new HttpRequestInput
        //            {
        //                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
        //                RequestMethod = Constants.HttpRequestMethods.Post,
        //                Url = string.Format("https://checkout-stg.easypaymentgateway.com/EPGCheckout/rest/status/merchantcall/repeat?{1}", url, CommonFunctions.GetUriDataFromObject(payoutStatusRequestInput))
        //            };

        //            var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        //            //if (output.Data.Status == 5)
        //            //{
        //            //    using (var documentBl = new DocumentBll(clientBl))
        //            //    {
        //            //        using (var notificationBl = new NotificationBll(clientBl))
        //            //        {
        //            //            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty, null, null, false, string.Empty, documentBl);
        //            //            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
        //            //        }
        //            //    }
        //            //}
        //            //else if (!WaitingStatuses.Contains(output.Data.Status))
        //            //    using (var documentBl = new DocumentBll(clientBl))
        //            //    {
        //            //        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
        //            //        output.Message, null, null, false, string.Empty, documentBl);
        //            //    }
        //        }
        //    }
        //}

        public static String sha256_hash(String value)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                return String.Join("", hash
                  .ComputeHash(Encoding.UTF8.GetBytes(value))
                  .Select(item => item.ToString("x2")));
            }
        }

        public static string AesEncryption(string data, string key)
        {
            using (var aes = new AesManaged { Key = Encoding.UTF8.GetBytes(key), Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
            {
                var crypt = aes.CreateEncryptor();
                byte[] encBytes = Encoding.UTF8.GetBytes(data);
                byte[] resultBytes = crypt.TransformFinalBlock(encBytes, 0, encBytes.Length);
                return Convert.ToBase64String(resultBytes);
            }
        }
    }
}