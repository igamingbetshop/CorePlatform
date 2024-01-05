using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.CardPay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class CardPayHelpers
    {
        public static string CallCardPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CardPayUrl).StringValue;
                var token = GetToken(client.PartnerId, Convert.ToInt32(partnerPaymentSetting.UserName), partnerPaymentSetting.Password);
                if (string.IsNullOrEmpty(token))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);               

                var paymentRequestInput = new PaymentRequestInput
                {
                    MerchantCustomer = new PaymentCustomer
                    {
                        Id = client.Id.ToString(),
                        //Email = client.Email,
                    },
                    MerchantOrder = new PaymentOrder
                    {
                        Id = input.Id.ToString(),
                        Description = input.Id.ToString()
                    },
                    PaymentDataDetails = new PaymentData
                    {
                        Amount = Convert.ToInt32(input.Amount),
                        Currency = client.CurrencyId
                    },
                    RequestDetails = new PaymentRequestDetails
                    {
                        Id = input.Id.ToString(),
                        RequestTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture)
                    },
                    ReturnUrl = new ReturnUrls
                    {
                        CancelUrl= cashierPageUrl,
                        DeclineUrl= cashierPageUrl,
                        ReturnUrl= cashierPageUrl,
                        SuccessUrl = cashierPageUrl
                    }
                };
                if (paymentsystem.Name == Constants.PaymentSystems.CardPayBank || paymentsystem.Name == Constants.PaymentSystems.CardPayCard)
                {
                    if (!string.IsNullOrEmpty(client.Email))
                        paymentRequestInput.MerchantCustomer.Email = client.Email;
                    else if (!string.IsNullOrEmpty(client.MobileNumber))
                        paymentRequestInput.MerchantCustomer.MobileNumber = client.MobileNumber;
                    else
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
                }
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                if (paymentsystem.Name == Constants.PaymentSystems.CardPayBank)
                {
                    var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                    if (bankInfo == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                    paymentRequestInput.PaymentMethod = bankInfo.BankCode;
                }
                else
                {
                    paymentRequestInput.PaymentMethod = PaymentWays[paymentsystem.Name];
                }
                
                Dictionary<string, string> requestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + token } };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/api/payments", url),
                    RequestHeaders = requestHeaders,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    })
                };
                log.Info(JsonConvert.SerializeObject(httpRequestInput));
                var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var paymentRequestOutput = JsonConvert.DeserializeObject<PaymentRequestOutput>(response);
                return paymentRequestOutput.RedirectUrl;
            }
        }

        private static string GetToken(int partnerId, int termCode, string termPass)
        {
            var tokenRequestInput = new TokenRequestInput
            {
                grant_type = "password",
                terminal_code = termCode,
                password = termPass
            };
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CardPayUrl).StringValue;

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = string.Format("{0}/api/auth/token", url),
                PostData = CommonFunctions.GetUriEndocingFromObject(tokenRequestInput)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var tokenRequestOutput = JsonConvert.DeserializeObject<TokenRequestOutput>(response);
            return tokenRequestOutput.access_token;
        }
        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var currencyBl = new CurrencyBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CardPayUrl).StringValue;
                    var token = GetToken(client.PartnerId, Convert.ToInt32(partnerPaymentSetting.UserName), partnerPaymentSetting.Password);
                    if (string.IsNullOrEmpty(token))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                    var amount = input.Amount - (input.CommissionAmount ?? 0);
                    var requestInput = new PayoutInput
                    {
                        MerchantOrder = new PaymentOrder
                        {
                            Id = input.Id.ToString(),
                            Description = input.Id.ToString()
                        },
                        PayoutData = new PaymentData
                        {
                            Amount = Convert.ToInt32(amount),
                            Currency = client.CurrencyId
                        },
                        Request = new PaymentRequestDetails
                        {
                            Id = input.Id.ToString(),
                            RequestTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    };

                    if (paymentsystem.Name == Constants.PaymentSystems.CardPayBank)
                    {
                        var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                        if (bankInfo == null)
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                        requestInput.PaymentMethod = bankInfo.BankCode;
                        requestInput.Account = new EWalletAccount
                        {
                            Id = paymentInfo.CardNumber,
                            BankBranch = bankInfo.BranchName
                        };
                    }
                    else
                    {
                        requestInput.PaymentMethod = PaymentWays[paymentsystem.Name];
                        if (paymentsystem.Name == Constants.PaymentSystems.CardPayCard)
                        {
                            requestInput.CardAccount = new CardAccount
                            {
                                Card = new CardObject
                                {
                                    Pan = paymentInfo.CardNumber,
                                    ExpireDate = paymentInfo.ExpirationDate
                                },
                                RecipientInfo = !string.IsNullOrEmpty(paymentInfo.CardHolderName) ? paymentInfo.CardHolderName : client.Id.ToString()
                            };
                            requestInput.Account = new EWalletAccount
                            {
                                Id = paymentInfo.CardNumber
                            };
                        }
                    }

                    Dictionary<string, string> requestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + token } };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = string.Format("{0}/api/payouts", url),
                        RequestHeaders = requestHeaders,
                        PostData = JsonConvert.SerializeObject(requestInput, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        })
                    };

                    var payoutOutput = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    input.ExternalTransactionId = payoutOutput.PayoutData.Id;
                    paymentSystemBl.ChangePaymentRequestDetails(input);

                    if (SuccessStatuses.Contains(payoutOutput.PayoutData.Status.ToUpper()))
                    {
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding,
                        };
                    }
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Failed,
                        Description = GetErrorDescription(payoutOutput.PayoutData.DeclineCode)
                    };
                }
            }
        }

        public static List<string> SuccessStatuses = new List<string>
        {
            "NEW",
            "IN_PROGRESS",
            "AUTHORIZED",
            "COMPLETED"
        };

        private static Dictionary<string, string> PaymentWays { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.CardPayCard, "BANKCARD" },
            { Constants.PaymentSystems.CardPayQIWI, "QIWI" },
            { Constants.PaymentSystems.CardPayYandex, "YANDEXMONEY" },
            { Constants.PaymentSystems.CardPayWebMoney, "WEBMONEY" },
            { Constants.PaymentSystems.CardPayBoleto, "BOLETO" },
            { Constants.PaymentSystems.CardPayLoterica, "LOTERICA" },
            { Constants.PaymentSystems.CardPaySpei, "SPEI" },
            { Constants.PaymentSystems.CardPayDirectBankingEU, "DIRECTBANKINGEU" }
        };

        public static Dictionary<string, string> DeclineCodes = new Dictionary<string, string>
        {
            {"01", "System malfunction"},
            {"02", "Cancelled by customer"},
            {"03", "Declined by Antifraud"},
            {"04", "Declined by 3-D Secur"},
            {"05", "Only 3-D Secure transactions are allowed"},
            {"06", "3-D Secure availability is unknown"},
            {"07", "Limit reached"},
            {"08", "Requested operation is not supported"},
            {"10", "Declined by bank (reason not specified)"},
            {"11", "Common decline by bank"},
            {"13", "Insufficient funds"},
            {"14", "Card limit reached"},
            {"15", "Incorrect card data"},
            {"16", "Declined by bank’s antifraud"},
            {"17", "Bank’s malfunction"},
            {"18", "Connection problem"},
            {"21", "General exception"}
        };

        public static string GetErrorDescription(string error)
        {
            if (DeclineCodes.ContainsKey(error))
                return DeclineCodes[error];
            return DeclineCodes[Constants.Errors.GeneralException.ToString()];
        }
    }
}
