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
using System.Collections.Generic;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Integration.Payments.Models.Jeton;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.DAL.Models.Cache;
using System.Text.RegularExpressions;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class JetonHelpers
    {
        private static Dictionary<string, string> PaymentMethods { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.JetonCheckout, "CHECKOUT" },
            { Constants.PaymentSystems.JetonDirect, "DIRECT" },
            { Constants.PaymentSystems.JetonQR, "QR" },
            { Constants.PaymentSystems.JetonGo, "JETGO" }
        };
        private static string QRPrefix = "data:image/gif;base64,";
        public static string CallJetonApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                if (!PaymentMethods.ContainsKey(paymentSystem.Name))
                    BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.JetonApiUrl);
                var resourcesUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResourcesUrl);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var amount = input.Amount;
                if (input.CurrencyId != Constants.Currencies.Euro)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.Euro);
                    amount = Math.Round(rate * input.Amount, 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.Euro);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                var paymentInput = new
                {
                    orderId = input.Id.ToString(),
                    amount = amount.ToString("F"),
                    currency = Constants.Currencies.Euro,
                    method = PaymentMethods[paymentSystem.Name],
                    returnUrl = cashierPageUrl,
                    language = session.LanguageId.ToUpper(),
                    customer = paymentInfo.CardNumber,
                    customerReferenceNo = client.Id.ToString()
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "X-API-KEY", partnerPaymentSetting.UserName } },
                    Url = string.Format("{0}/pay", url),
                    PostData = JsonConvert.SerializeObject(paymentInput)
                };
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                input.ExternalTransactionId = paymentOutput.PaymentId;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                if (!string.IsNullOrEmpty(paymentOutput.Message))
                    throw new Exception($"ErrorCode: {paymentOutput.Code}  Message: {paymentOutput.Message}");
                if (paymentSystem.Name == Constants.PaymentSystems.JetonQR)
                {
                    var paymentProcessingInput = new
                    {
                        PayAddress = QRPrefix + paymentOutput.QR,
                        Amount = paymentInput.amount,
                        Currency = paymentInput.currency,
                        PartnerDomain = session.Domain,
                        ResourcesUrl = (resourcesUrlKey == null || resourcesUrlKey.Id == 0 ? ("https://resources." + session.Domain) : resourcesUrlKey.StringValue),
                        PartnerId = client.PartnerId,
                        LanguageId = session.LanguageId,
                        PaymentSystemName = "qrprocessing"
                    };

                    var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                    if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                        distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                    var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
                    var data = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(paymentProcessingInput));
                    return string.Format("{0}/paymentform/paymentprocessing?data={1}", distributionUrl, data);
                }
                return paymentOutput.Checkout;
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.JetonApiUrl);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                if (paymentRequest.CurrencyId != Constants.Currencies.Euro)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.Euro);
                    amount = Math.Round(rate * paymentRequest.Amount, 2);
                    var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                    parameters.Add("Currency", Constants.Currencies.Euro);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                }
                var payoutInput = new
                {
                    orderId = paymentRequest.Id.ToString(),
                    amount = amount.ToString("F"),
                    currency = Constants.Currencies.Euro,
                    customer = paymentInfo.WalletNumber
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "X-API-KEY", partnerPaymentSetting.UserName } },
                    Url = string.Format("{0}/payout", url),
                    PostData = JsonConvert.SerializeObject(payoutInput)
                };
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                paymentRequest.ExternalTransactionId =  paymentOutput.PaymentId;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                if (!string.IsNullOrEmpty(paymentOutput.PaymentId))
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                    };
                throw new Exception("error");
            }
        }

        public static PaymentResponse PayVoucher(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                  input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var amount = input.Amount;
                    if (input.CurrencyId != Constants.Currencies.Euro)
                    {
                        var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.Euro);
                        amount = Math.Round(rate * input.Amount, 2);
                        var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                         JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                        parameters.Add("Currency", Constants.Currencies.Euro);
                        parameters.Add("AppliedRate", rate.ToString("F"));
                        input.Parameters = JsonConvert.SerializeObject(parameters);
                        paymentSystemBl.ChangePaymentRequestDetails(input);
                    }
                    var token = GetToken(partnerPaymentSetting.UserName, input, amount, session.Country, client, cashierPageUrl, session.LanguageId);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    if (!Regex.IsMatch(paymentInfo.ExpirationDate, "(0[1-9]|10|11|12)/202[3-9]$"))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);

                    var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.JetonCashApiUrl);

                    var paymentInput = new
                    {
                        account = new
                        {
                            voucherNumber = paymentInfo.VoucherNumber,
                            pin = paymentInfo.ActivationCode,
                            expDate = paymentInfo.ExpirationDate.Replace('/', '-')
                        },
                        customerInfo = new
                        {
                            ip = session.LoginIp,
                            agent = session.Source
                        },
                        paymentMethod = "REDEEM_VOUCHER"
                    };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        RequestHeaders = new Dictionary<string, string> { { "Authorization", token } },
                        Url = string.Format("{0}/pay", url),
                        PostData = JsonConvert.SerializeObject(paymentInput)
                    };
                    var r = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    var voucherOutput = JsonConvert.DeserializeObject<VoucherOutput>(r);
                    input.ExternalTransactionId = voucherOutput.TransactionId;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    if (voucherOutput.Status.ToUpper()!="APPROVED")
                        throw new Exception($"Code: {voucherOutput.Code},  Status: {voucherOutput.Status}, Message: {voucherOutput.Message}");
                    return new PaymentResponse
                    {
                        Description = "Success"
                    };
                }
            }
        }

        private static string GetToken(string apiKey, PaymentRequest paymentRequest, decimal amount, string countryCode, BllClient client, string cashierUrl, string lang)
        {
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                Constants.PartnerKeys.JetonCashApiUrl);
            var input = new
            {
                amount,
                apiKey,
                country = countryCode,
                currency = Constants.Currencies.Euro,
                dateOfBirth = client.BirthDate.ToString("yyyy-MM-dd"),
                defaultPaymentMethod = "REDEEM_VOUCHER",
                email = client.Email,
                failRedirectUrl = cashierUrl,
                firstName = client.FirstName,
                lastName = client.LastName,
                language = lang.ToUpper(),
                referenceNo = paymentRequest.Id.ToString(),
                successRedirectUrl = cashierUrl
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = string.Format("{0}/initialize", url),
                PostData = JsonConvert.SerializeObject(input)
            };
            var tokenOutput = JsonConvert.DeserializeObject<TokenOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (tokenOutput.Code != "00000")
                throw new Exception(tokenOutput.Message);
            return tokenOutput.Token;
        }

        public static PaymentResponse CreateVoucher(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (client.CurrencyId != Constants.Currencies.Euro)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.JetonCashoutApiUrl);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                    var tokenInput = new { apiKey = partnerPaymentSetting.Password };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = string.Format("{0}/auth/merchant/token", url),
                        PostData = JsonConvert.SerializeObject(tokenInput)
                    };
                    var tokenOutput = JsonConvert.DeserializeObject<TokenOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                    if (paymentRequest.CurrencyId != Constants.Currencies.Euro)
                    {
                        var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.Euro);
                        amount = Math.Round(rate * paymentRequest.Amount, 2);
                        var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                                      JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                        parameters.Add("Currency", Constants.Currencies.Euro);
                        parameters.Add("AppliedRate", rate.ToString("F"));
                        paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    }
                    var voucherInput = new
                    {
                        amount,
                        country = session.Country,
                        currency = Constants.Currencies.Euro,
                        email = client.Email,
                        firstName = client.FirstName,
                        lastName = client.LastName,
                        referenceNo = paymentRequest.Id.ToString()
                    };
                    httpRequestInput.RequestHeaders = new Dictionary<string, string> { { "Authorization", tokenOutput.AccessToken } };
                    httpRequestInput.PostData = JsonConvert.SerializeObject(voucherInput);
                    httpRequestInput.Url =  string.Format("{0}/withdraw/voucher/jetonCash", url);
                    var voucherOutput = JsonConvert.DeserializeObject<CreateVoucherOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    if (voucherOutput.Status.ToUpper() != "APPROVED")
                        throw new Exception($"Code: {voucherOutput.Code},  Status: {voucherOutput.Status}, Message: {voucherOutput.Message}");
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            paymentRequest.ExternalTransactionId = voucherOutput.TransactionId;
                            paymentRequest.Info = JsonConvert.SerializeObject(
                            new PaymentInfo
                            {
                                VoucherNum = voucherOutput.VoucherNumber,
                                VoucherCode = voucherOutput.SecureCode,
                                VoucherAmount = voucherOutput.Amount.ToString("F"),
                                ExpiryDate = new DateTime(voucherOutput.ExpiryYear, voucherOutput.ExpiryMonth, 1).ToString(),
                            },
                            new JsonSerializerSettings()
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Ignore
                            });
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, 
                                string.Empty, null, null, false, string.Empty, documentBl, notificationBl);
                            return new PaymentResponse
                            {
                                Status = PaymentRequestStates.PayPanding,
                            };
                        }
                    }
                }
            }
        }
    }
}