using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.PaySec;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class PaySecHelpers
    {
        public static string CallPaySecApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var merchantCode = partnerPaymentSetting.UserName;
                var merchantKey = partnerPaymentSetting.Password;
                if (input.Amount <= 10)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongOperationAmount);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaySecUrl).StringValue + "payIn/";
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                if (bankInfo == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                var requestInput = new PaymentRequestInput
                {
                    inputHeader = new InputHeader
                    {
                        Version = "3.0",
                        MerchantCode = merchantCode
                    },
                    inputBody = new InputBody
                    {
                        ChannelCode = "BANK_TRANSFER",
                        BankCode = bankInfo.BankCode,
                        NotifyURL = string.Format("{0}/{1}", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue, "api/PaySec/ApiRequest"),
                        ReturnURL = string.Format("https://{0}", session.Domain),
                        Amount = input.Amount.ToString("F"),
                        OrderTime = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString(),
                        TransactionId = input.Id.ToString(),
                        CurrencyId = input.CurrencyId
                    }
                };
                var signature = string.Format("{0};{1};{2};{3};{4}",
                                       requestInput.inputBody.TransactionId, requestInput.inputBody.Amount,
                                       requestInput.inputBody.CurrencyId, requestInput.inputHeader.MerchantCode,
                                       requestInput.inputHeader.Version);
                signature = CommonFunctions.ComputeSha256(signature);
                requestInput.inputHeader.Signature = BCrypt.Net.BCrypt.HashPassword(signature, merchantKey).Replace(merchantKey, string.Empty);

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    Url = url + "requestToken",
                    PostData = JsonConvert.SerializeObject(requestInput)
                };

                var output = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                var requestUrl = string.Format(CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl).StringValue, session.Domain);
                if (output.outputHeader.Status.ToUpper() == "SUCCESS")
                    return string.Format("{0}/apcopay/PaymentRequest?endpoint={1}&params={2}", requestUrl, url, output.outputBody.Token);
                throw new Exception(output.outputHeader.Message);
            }
        }

        public static PaymentResponse CreatePayment(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var paymentSystemBl = new PaymentSystemBll(partnerBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId);
                    if (client == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);

                    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PaySec);
                    if (paymentSystem == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    if (partner == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerNotFound);
                    var url = partnerBl.GetPaymentValueByKey(null, input.PaymentSystemId, Constants.PartnerKeys.PaySecUrl) + "payOut/";
                    var merchantCode = partnerPaymentSetting.UserName;
                    var merchantKey = partnerPaymentSetting.Password;
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                    if(bankInfo==null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                    var range = PayoutLimits[input.CurrencyId];
                    if (input.Amount > range.Value || input.Amount < range.Key)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentRequestInValidAmount);
                    var amount = input.Amount - (input.CommissionAmount ?? 0);

                    var requestInput = new PayoutRequestInput
                    {
                        MerchantCode = merchantCode,
                        Amount = /*input.CurrencyId == "IDR" ? input.Amount.ToString() :*/ amount.ToString("F"),
                        Currency = input.CurrencyId,
                        BankCode = bankInfo.BankCode,
                        BankName = bankInfo.BankName,
                        BankBranch = bankInfo.BranchName,
                        CustomerName = client.FirstName,
                        BankAccountName = paymentInfo.BankName,
                        BankAccountNumber = paymentInfo.BankAccountNumber,
                        TransactionId = input.Id.ToString(),
                        Province = "北京",
                        City = input.CurrencyId == "CNY" ? "北京" : "Jakarta",
                        Version = "3.0",
                        NotifyURL = partnerBl.GetPaymentValueByKey(client.PartnerId, null, Constants.PartnerKeys.PaymentGateway) + "/api/PaySec/PayoutResult"
                    };
                    var signature = string.Format("{0};{1};{2};{3};{4}",
                                          requestInput.TransactionId, amount,
                                          requestInput.Currency, requestInput.MerchantCode,
                                          requestInput.Version);
                    signature = CommonFunctions.ComputeSha256(signature);
                    requestInput.Signature = BCrypt.Net.BCrypt.HashPassword(signature, merchantKey).Replace(merchantKey, string.Empty);
                  
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = System.Net.Http.HttpMethod.Post,
                        Url = url + "paymentRequest",
                        PostData = JsonConvert.SerializeObject(requestInput)
                    };

                    var output = JsonConvert.DeserializeObject<SendPayoutTokenOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));

                    var response = new PaymentResponse
                    {
                        Status = output.Status.ToUpper() == "PENDING" ? PaymentRequestStates.PayPanding : PaymentRequestStates.Failed
                    };
                    response.Description = output.StatusMessage;
                    return response;
                }
            }
        }

        private static string ComputeHash(string key, string value)
        {
            var byteKey = Encoding.UTF8.GetBytes(key);
            string hashString;

            using (var hmac = new HMACSHA256(byteKey))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
                hashString = Convert.ToBase64String(hash);
            }
            return hashString;
        }

        private static Dictionary<string, KeyValuePair<decimal, decimal>> PayoutLimits = new Dictionary<string, KeyValuePair<decimal, decimal>>
        {
            {"CNY", new KeyValuePair<decimal, decimal>((decimal)60.00, (decimal)49000.00)},
            {"THB", new KeyValuePair<decimal, decimal>((decimal)350.00, (decimal)175000.00)},
            {"IDR", new KeyValuePair<decimal, decimal>((decimal)50000.00, (decimal)25000000.00)},
            {"MYR", new KeyValuePair<decimal, decimal>((decimal)50.00, (decimal)20000.00)},
            {"VND", new KeyValuePair<decimal, decimal>(10000, 1000000000)}
        };
    }
}
