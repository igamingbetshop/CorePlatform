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
using IqSoft.CP.Integration.Payments.Models.MaldoPay;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class MaldoPayHelpers
    {
        public static Dictionary<string, int> PaymentServices { get; set; } = new Dictionary<string, int>
        {
            { Constants.PaymentSystems.MaldoPayHavale, 2166},
            { Constants.PaymentSystems.MaldoPayBankTransfer, 2006},
            { Constants.PaymentSystems.MaldoPayInstantlyPapara, 2044}, // AnindaPapara
            { Constants.PaymentSystems.MaldoPayPapara, 2038},
            { Constants.PaymentSystems.MaldoPayCrypto, 2047},
            { Constants.PaymentSystems.MaldoPayCreditCard, 2031},
            { Constants.PaymentSystems.MaldoPayMefete, 2093},
            { Constants.PaymentSystems.MaldoPayPayFix, 2078},
            { Constants.PaymentSystems.MaldoPayPix, 2146},
            { Constants.PaymentSystems.MaldoPayParazula, 2150},
            { Constants.PaymentSystems.MaldoPayQR, 2051}
        };

        private static Dictionary<string, int> PayoutServices { get; set; } = new Dictionary<string, int>
        {
            { Constants.PaymentSystems.MaldoPayBankTransfer, 2012},
            { Constants.PaymentSystems.MaldoPayPapara, 2040},
            { Constants.PaymentSystems.MaldoPayMefete, 2095},
            { Constants.PaymentSystems.MaldoPayPayFix, 2079},
            { Constants.PaymentSystems.MaldoPayParazula, 2149},
            { Constants.PaymentSystems.MaldoPayPix, 2147},
            { Constants.PaymentSystems.MaldoPayCryptoBTC, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoETH, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoETHBEP20, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoLTC, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoXRP, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoXLM, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoBCH, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoLINKERC20, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoUSDCERC20, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoUSDCBEP20, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoUSDCTRC20, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoUSDTERC20, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoUSDTBEP20, 2057},
            { Constants.PaymentSystems.MaldoPayCryptoUSDTTRC20, 2057}
        };

        private static Dictionary<string, KeyValuePair<string, string>> CryptoNetworks { get; set; } = new Dictionary<string, KeyValuePair<string, string>>
        {
            { Constants.PaymentSystems.MaldoPayCryptoBTC, new KeyValuePair<string,string> ("BTC", "BTC")},
            { Constants.PaymentSystems.MaldoPayCryptoETH, new KeyValuePair<string,string> ("ETH", "ETH")},
            { Constants.PaymentSystems.MaldoPayCryptoETHBEP20, new KeyValuePair<string,string> ("BSC", "ETH")},
            { Constants.PaymentSystems.MaldoPayCryptoLTC, new KeyValuePair<string,string> ("LTC", "LTC")},
            { Constants.PaymentSystems.MaldoPayCryptoXRP, new KeyValuePair<string,string> ("XRP", "XRP")},
            { Constants.PaymentSystems.MaldoPayCryptoXLM, new KeyValuePair<string,string> ("XLM", "XLM")},
            { Constants.PaymentSystems.MaldoPayCryptoBCH, new KeyValuePair<string,string> ("BCH", "BCH")},
            { Constants.PaymentSystems.MaldoPayCryptoLINKERC20, new KeyValuePair<string,string> ("ETH", "LINK")},
            { Constants.PaymentSystems.MaldoPayCryptoUSDCERC20, new KeyValuePair<string,string> ("ETH", "USDC")},
            { Constants.PaymentSystems.MaldoPayCryptoUSDCBEP20, new KeyValuePair<string,string> ("BSC", "USDC")},
            { Constants.PaymentSystems.MaldoPayCryptoUSDCTRC20, new KeyValuePair<string,string> ("TRX", "USDC")},
            { Constants.PaymentSystems.MaldoPayCryptoUSDTERC20, new KeyValuePair<string,string> ("ETH", "USDT")},
            { Constants.PaymentSystems.MaldoPayCryptoUSDTBEP20, new KeyValuePair<string,string> ("BSC", "USDT")},
            { Constants.PaymentSystems.MaldoPayCryptoUSDTTRC20, new KeyValuePair<string,string> ("TRX", "USDT")},
        };

        public static string CallMaldoPay(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            log.Info("client: " + JsonConvert.SerializeObject(client));
            if (client.CurrencyId != Constants.Currencies.TurkishLira)
               throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);

            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);

            var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            if (!PaymentServices.ContainsKey(paymentSystem.Name))
                BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);

            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var merchant = partnerPaymentSetting.UserName.Split(',');
            var mid = merchant[0];
            var brandId = merchant[1];
            var integrationId = merchant[2];
            var keys = partnerPaymentSetting.Password.Split(',');
            var encryptionKey = keys[0];
            var apiKey = keys[1];
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MaldoPayApiUrl).StringValue;
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            object serviceData = null;
            if (paymentSystem.Name ==  Constants.PaymentSystems.MaldoPayCreditCard)
            {
                serviceData = new
                {
                    serviceData1 = paymentInfo.NationalId, //MaldoCard  NationalId Customer CPF.
                    locale = session.LanguageId
                };
            }
            else if (paymentSystem.Name ==  Constants.PaymentSystems.MaldoPayCreditCard)
            {
                serviceData = new
                {
                    serviceData3 = paymentInfo.Info, //MaldoPix Customer CPF.
                };
            }

            var paymentJson = new
            {
                transaction = new
                {
                    clientId = mid,
                    brandId,
                    integrationId,
                    landingPages = new
                    {
                        landingSuccess = cashierPageUrl,
                        landingPending = cashierPageUrl,
                        landingDeclined = cashierPageUrl,
                        landingFailed = cashierPageUrl
                    },
                    request = new
                    {
                        serviceId = PaymentServices[paymentSystem.Name],
                        currencyCode = client.CurrencyId,
                        amount = (int)input.Amount,
                        referenceOrderId = input.Id.ToString(),
                        serviceData
                    },
                    user = new
                    {
                        firstName = client.FirstName,
                        lastName = client.LastName,
                        birthDate = client.BirthDate.ToString("yyyy-MM-dd"),
                        countryCode = session.Country,
                        playerId = client.Id.ToString(),
                        postCode = string.IsNullOrWhiteSpace(client.ZipCode.Trim()) ? "dummy" : client.ZipCode.Trim(),
                        languageCode = session.LanguageId,
                        emailAddress = client.Email
                    }
                }
            };

            var paymentInput = new
            {
                json = JsonConvert.SerializeObject(paymentJson),
                checksum = CommonFunctions.ComputeHMACSha256(JsonConvert.SerializeObject(paymentJson), encryptionKey).ToLower(),
                apiKey
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                Accept = $"{Constants.HttpContentTypes.ApplicationJson}; version=2.1",
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                PostData = CommonFunctions.GetUriDataFromObject(paymentInput)
            };
            log.Info(JsonConvert.SerializeObject(httpRequestInput));
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            log.Info(JsonConvert.SerializeObject(response));
            var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(response);

            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                input.ExternalTransactionId =  paymentOutput.TransactionId;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                if (!string.IsNullOrEmpty(paymentOutput.Redirect))
                    return paymentOutput.Redirect;
            }
            throw new Exception(response);
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            if (paymentRequest.CurrencyId != Constants.Currencies.TurkishLira)
                BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                               paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MaldoPayApiUrl).StringValue;
            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
            if (!PayoutServices.ContainsKey(paymentSystem.Name))
                BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
            var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var merchant = partnerPaymentSetting.UserName.Split(',');
                var mid = merchant[0];
                var brandId = merchant[1];
                var integrationId = merchant[2];
                var keys = partnerPaymentSetting.Password.Split(',');
                var encryptionKey = keys[0];
                var apiKey = keys[1];
                object serviceData = null;
                switch (paymentSystem.Name)
                {
                    case Constants.PaymentSystems.MaldoPayBankTransfer:
                        if (string.IsNullOrEmpty(paymentInfo.BankId))
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                        var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId)) ??
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                        serviceData = new
                        {
                            serviceData1 = "s2s",
                            serviceData2 = bankInfo.BankCode,
                            serviceData3 = paymentInfo.DocumentId,
                            serviceData4 = paymentInfo.MobileNumber,
                            serviceData5 = paymentInfo.AccountNumber,
                            serviceData6 = paymentInfo.BeneficiaryName,
                            serviceData7 = paymentInfo.BankAccountNumber,
                        };
                        break;
                    case Constants.PaymentSystems.MaldoPayPapara:
                    case Constants.PaymentSystems.MaldoPayPix:
                    case Constants.PaymentSystems.MaldoPayParazula:
                        serviceData = new
                        {
                            serviceData1 = "s2s",
                            serviceData3 = paymentInfo.WalletNumber
                        };
                        break;
                    case Constants.PaymentSystems.MaldoPayPayFix:
                        serviceData = new
                        {
                            serviceData1 = "s2s",
                            serviceData3 = paymentInfo.WalletNumber, //PayFix user identification number
                            serviceData4 = paymentInfo.AccountType // PayFix user name
                        };
                        break;
                    case Constants.PaymentSystems.MaldoPayMefete:
                        serviceData = new
                        {
                            serviceData1 = "s2s",
                            serviceData3 = paymentInfo.WalletNumber, //MEFETE account number
                            serviceData8 = paymentInfo.AccountType //  MFT Turkish ID in case of sts
                        };
                        break;
                    case Constants.PaymentSystems.MaldoPayCryptoBTC:
                    case Constants.PaymentSystems.MaldoPayCryptoETH:
                    case Constants.PaymentSystems.MaldoPayCryptoETHBEP20:
                    case Constants.PaymentSystems.MaldoPayCryptoLTC:

                    case Constants.PaymentSystems.MaldoPayCryptoBCH:
                    case Constants.PaymentSystems.MaldoPayCryptoLINKERC20:
                    case Constants.PaymentSystems.MaldoPayCryptoUSDCERC20:
                    case Constants.PaymentSystems.MaldoPayCryptoUSDCBEP20:
                    case Constants.PaymentSystems.MaldoPayCryptoUSDCTRC20:
                    case Constants.PaymentSystems.MaldoPayCryptoUSDTERC20:
                    case Constants.PaymentSystems.MaldoPayCryptoUSDTBEP20:
                    case Constants.PaymentSystems.MaldoPayCryptoUSDTTRC20:
                        var channel = CryptoNetworks[paymentSystem.Name];
                        serviceData = new
                        {
                            serviceData1 = "s2s",
                            serviceData3 = paymentInfo.WalletNumber,
                            serviceData4 = channel.Value,                           
                            serviceData6 = channel.Key
                        };
                        break;
                    case Constants.PaymentSystems.MaldoPayCryptoXRP:
                    case Constants.PaymentSystems.MaldoPayCryptoXLM:
                        channel = CryptoNetworks[paymentSystem.Name];
                        serviceData = new
                        {
                            serviceData1 = "s2s",
                            serviceData3 = paymentInfo.WalletNumber,
                            serviceData4 = channel.Value,
                            serviceData5 = paymentInfo.AccountType, // Destination Tag
                            serviceData6 = channel.Key
                        };
                        break;
                    default: break;
                }
                var paymentJson = new
                {
                    transaction = new
                    {
                        clientId = mid,
                        brandId,
                        integrationId,
                        request = new
                        {
                            serviceId = PayoutServices[paymentSystem.Name].ToString(),
                            currencyCode = client.CurrencyId,
                            amount = amount.ToString("F"),
                            referenceOrderId = paymentRequest.Id.ToString(),
                            serviceData
                        },
                        user = new
                        {
                            firstName = client.FirstName,
                            lastName = client.LastName,
                            birthDate = client.BirthDate.ToString("yyyy-MM-dd"),
                            countryCode = paymentInfo.Country,
                            playerId = client.Id.ToString(),
                            postCode = client.ZipCode.Trim(),
                            languageCode = session.LanguageId,
                            emailAddress = client.Email
                        }
                    }
                };

                var paymentInput = new
                {
                    json = JsonConvert.SerializeObject(paymentJson),
                    checksum = CommonFunctions.ComputeHMACSha256(JsonConvert.SerializeObject(paymentJson), encryptionKey).ToLower(),
                    apiKey
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    Accept = $"{Constants.HttpContentTypes.ApplicationJson}; version=2.1",
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    PostData = CommonFunctions.GetUriDataFromObject(paymentInput)
                };
                log.Info(JsonConvert.SerializeObject(httpRequestInput));
                var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                log.Info(JsonConvert.SerializeObject(response));
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(response);
                paymentRequest.ExternalTransactionId =  paymentOutput?.TransactionData?.TransactionId;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                if (paymentOutput?.TransactionData == null || paymentOutput.TransactionData.CodeId.ToString().StartsWith("4") ||
                    paymentOutput.TransactionData.CodeId.ToString().StartsWith("5"))
                    return new PaymentResponse
                    {
                        Description = paymentOutput?.TransactionData?.CodeMessage,
                        Status = PaymentRequestStates.Failed,
                    };
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
        }
    }
}