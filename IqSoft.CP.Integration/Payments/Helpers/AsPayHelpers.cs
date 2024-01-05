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
using IqSoft.CP.Common.Helpers;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models.AsPay;
using IqSoft.CP.Integration.Payments.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class AsPayHelpers
    {
        private static Dictionary<string, KeyValuePair<int, string>> PaymentServices { get; set; } = new Dictionary<string, KeyValuePair<int, string>>
        {
            { Constants.PaymentSystems.AsPayHavale, new KeyValuePair<int, string> (106,"payhavale") },
            { Constants.PaymentSystems.AsPayCreditCard,new KeyValuePair<int, string> (0, "paycc") },
            { Constants.PaymentSystems.AsPayHoppa, new KeyValuePair<int, string> (110, "hoppa" )},
            { Constants.PaymentSystems.AsPayPapara, new KeyValuePair<int, string> (108, "papara") }
        };

        private static Dictionary<string, KeyValuePair<int, string>> PayoutServices { get; set; } = new Dictionary<string, KeyValuePair<int, string>>
        {
            { Constants.PaymentSystems.AsPayHavale, new KeyValuePair<int, string> (107,"payhavale") },
            { Constants.PaymentSystems.AsPayCreditCard,new KeyValuePair<int, string> (0, "paycc") },
            { Constants.PaymentSystems.AsPayHoppa, new KeyValuePair<int, string> (111, "hoppa" )},
            { Constants.PaymentSystems.AsPayPapara, new KeyValuePair<int, string> (109, "papara") }
        };

        public static string CallAsPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (client.CurrencyId != Constants.Currencies.TurkishLira)
                BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);
            if (string.IsNullOrEmpty(client.Email))
                BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (string.IsNullOrEmpty(client.FirstName))
                BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.AsPayApiUrl);
            var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            if (!PaymentServices.ContainsKey(paymentSystem.Name))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);
            var token = GetToken(partnerPaymentSetting.UserName, partnerPaymentSetting.Password, url);
            var paymentInput = new
            {
                input.Amount,
                EndPointUrl = $"{paymentGateway}/api/AsPay/ApiRequest",
                ProviderTrxId = input.Id.ToString(),
                RedirectUrlIsSuccess = cashierPageUrl,
                RedirectUrlIsError = cashierPageUrl,
                EMail = client.Email,
                Type = PaymentServices[paymentSystem.Name].Key,
                UserName = $"{client.FirstName} {client.LastName}",
                UserId = client.Id.ToString()
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "Token", token }, { "PKey", partnerPaymentSetting.Password } },
                Url = $"{url}/api/{PaymentServices[paymentSystem.Name].Value}",
                PostData = JsonConvert.SerializeObject(paymentInput)
            };
            var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (paymentOutput.IsSuccess)
            {
                using (var paymentSystemBl = new PaymentSystemBll(session, log))
                {
                    input.ExternalTransactionId =  paymentOutput.TransactionGuid;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    return paymentOutput.RedirectUrl;
                }
            }
            throw new Exception(paymentOutput.Message);
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            if (client.CurrencyId != Constants.Currencies.TurkishLira)
                BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);
            if (string.IsNullOrEmpty(client.Email))
                BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);


            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.AsPayApiUrl).StringValue;
            var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
            if (!PayoutServices.ContainsKey(paymentSystem.Name))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);
            var token = GetToken(partnerPaymentSetting.UserName, partnerPaymentSetting.Password, url);
            var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var paymentInput = new
            {
                Amount = amount,
                EndPointUrl = $"{paymentGateway}/api/AsPay/ApiRequest",
                ProviderTrxId = paymentRequest.Id.ToString(),
                EMail = client.Email,
                Type = PayoutServices[paymentSystem.Name].Key,
                UserName = $"{client.FirstName} {client.LastName}",
                IBAN = paymentInfo.WalletNumber,
                WalletID = paymentInfo.WalletNumber,
                UserId = client.Id.ToString()
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "Token", token }, { "PKey", partnerPaymentSetting.Password } },
                Url = $"{url}/api/{PayoutServices[paymentSystem.Name].Value}",
                PostData = JsonConvert.SerializeObject(paymentInput)
            };

            var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (paymentOutput.IsSuccess)
            {
                using (var paymentSystemBl = new PaymentSystemBll(session, log))
                {
                    paymentRequest.ExternalTransactionId =  paymentOutput.TransactionGuid;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                    };
                }
            }
            throw new Exception(paymentOutput.Message);
        }

        public static string GetToken(string apiKey, string privateKey, string url)
        {
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = $"{url}/api/gettoken",
                PostData = JsonConvert.SerializeObject( new { ApiKey = apiKey, PKey = privateKey})
            };
            var tokenResult = JsonConvert.DeserializeObject<TokenOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (tokenResult.IsSuccess)
                return tokenResult.Token;
            throw new Exception(tokenResult.Message);
        }
    }
}
