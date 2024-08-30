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
using IqSoft.CP.Integration.Payments.Models.XcoinsPay;
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class XcoinsPayHelpers
    {
        private static readonly Dictionary<string, string> CryptoNetworks = new Dictionary<string, string>
        {
            { "BTC", "bitcoin"},
            { "BCH", "bitcoin_cash"},
            { "DOGE", "DOGE"},
            { "ETH", "ethereum"},
            { "LTC" ,"litecoin" },
            { "TRX" ,"tron" },
            { "USDT" ,"tron" },
            { "USDTERC20" ,"ethereum" },
        };
        public static string CallXcoinsPayApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);

            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            using (var regionBl = new RegionBll(paymentSystemBl))
            {
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.XcoinsPayApiUrl);
                var region = regionBl.GetRegionByCountryCode(session.Country);
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var paymentInput = new
                {
                    merchantRef = partnerPaymentSetting.UserName,
                    userRef = client.Id.ToString(),
                    transactionRef = input.Id.ToString(),
                    fullName = $"{client.FirstName} {client.LastName}",
                    currencyCode = client.CurrencyId,
                    amount = (int)input.Amount,
                    country = region.NickName,
                    dateOfBirth = client.BirthDate.ToString("yyyy-MM-dd"),
                    onrEnabled = paymentSystem.Name == Constants.PaymentSystems.XcoinsPayCard,
                    zipCode = "dummy",
                    email = client.Email,
                    phoneCode = "",
                    phoneNumber = "",
                    state = region.NickName,
                    city = "dummy",
                    address = "dummy",
                    addressTwo = "dummy",
                    addressNumber = "1"
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password } },
                    Url = $"{url}/initiate-deposit",
                    PostData = JsonConvert.SerializeObject(paymentInput)
                };
                var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var res = JsonConvert.DeserializeObject<PaymentOutput>(response);
                if (string.IsNullOrEmpty(res.Data?.OrderId))
                    throw new Exception(response);
                var redirectUrl = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.XcoinsPayRedirectUrl);
                return redirectUrl + res.Data.OrderId;
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            using (var regionBl = new RegionBll(paymentSystemBl))
            {
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                      client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.XcoinsPayApiUrl);
                var privateKey = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.XcoinsPayApiPrivateKey);
                var region = regionBl.GetRegionByCountryCode(session.Country);
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                if (!CryptoNetworks.ContainsKey(paymentInfo.AccountType))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);
                var payoutJsonInput = JsonConvert.SerializeObject(new
                {
                    userRef = client.Id.ToString(),
                    merchantRef = partnerPaymentSetting.UserName,
                    transactionRef = paymentRequest.Id.ToString(),
                    fullName = $"{client.FirstName} {client.LastName}",
                    currencyCode = client.CurrencyId,
                    amount = (int)amount,
                    state = region.NickName,
                    city = client.City,
                    country = paymentInfo.Country,
                    dateOfBirth = client.BirthDate.ToString("yyyy-MM-dd"),
                    walletAddress = paymentInfo.WalletNumber,
                    coinType = paymentInfo.AccountType.Replace("USDTERC20", "USDT"),
                    blockchainNetwork = CryptoNetworks[paymentInfo.AccountType]
                });
                var headers = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password } };
                var nonce = Guid.NewGuid().ToString();
                headers.Add("Nonce", nonce);
                headers.Add("Xcoins-Signature", GenerateSignature(payoutJsonInput, nonce, privateKey));

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = headers,
                    Url = $"{url}/initiate-withdrawal",
                    PostData = payoutJsonInput
                };
                var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var payoutOutput = JsonConvert.DeserializeObject<PaymentOutput>(response);

                paymentRequest.ExternalTransactionId = payoutOutput?.Data?.OrderId;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                if (!string.IsNullOrEmpty(payoutOutput?.Data?.OrderId) && payoutOutput.StatusCode == 201)
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                    };
                throw new Exception(response);
            }
        }
        public static string GenerateSignature(string body,string nonce, string privateKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);

                byte[] dataToSign = Encoding.UTF8.GetBytes(body + nonce);
                byte[] signatureBytes = rsa.SignData(dataToSign, new SHA512CryptoServiceProvider());

                return (Convert.ToBase64String(signatureBytes));
            }
        }

      
    }
}
