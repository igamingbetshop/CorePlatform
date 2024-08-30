using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Zippy;
using Jose;
using log4net;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class ZippyHelpers
    {
        public static Dictionary<string, string> PaymentWays { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.ZippyPayIn, "cash" },
            { Constants.PaymentSystems.ZippyCard, "bankCard" },
            { Constants.PaymentSystems.ZippyBankTransfer, "bankTransfer" },
        };

        public static Dictionary<string, string> CountryCodesByCurrency { get; set; } = new Dictionary<string, string>
        {
            { Constants.Currencies.PeruvianSol, "PE" },
            { Constants.Currencies.MexicanPeso, "MX" },
            { Constants.Currencies.ChileanPeso, "CL" },
            { Constants.Currencies.USADollar, "EC" },
            { Constants.Currencies.BrazilianReal, "BR" }
        };

        public enum PaymentAccountTypes
        {
            CCT = 1,
            CTV = 2,
            CRUT = 3,
            AHO = 4,
            CCE = 5
        }

        public static string CallZippyCashInApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                if (string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidEmail);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.ZippyApiUrl).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var amount = input.Amount;
                if (input.CurrencyId != Constants.Currencies.ChileanPeso)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.ChileanPeso);
                    amount = Math.Round(rate * input.Amount, 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.ChileanPeso);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                var clientName = (!string.IsNullOrEmpty(client.FirstName) || !string.IsNullOrEmpty(client.LastName)) ?
                                  string.Format("{0} {1}", client.FirstName, client.LastName) : client.UserName;
                return string.Format("{0}/v1/cashin/{1}/{2}/{3}/{4}/{5}/{6}", url, clientName, paymentInfo.NationalId, client.Email,
                    Convert.ToInt32(amount), partnerPaymentSetting.UserName, input.Id);
            }
        }

        public static string CallZippyWebpayApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                if (string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidEmail);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.ZippyApiUrl).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var amount = input.Amount;
                if (input.CurrencyId != Constants.Currencies.ChileanPeso)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.ChileanPeso);
                    amount = rate * input.Amount;
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.ChileanPeso);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                var clientName = (!string.IsNullOrEmpty(client.FirstName) || !string.IsNullOrEmpty(client.LastName)) ?
                                    string.Format("{0} {1}", client.FirstName, client.LastName) : client.UserName;
                return string.Format("{0}/v1/webpay/{1}/{2}/{3}/{4}/{5}/{6}", url, clientName.ToUpper(), paymentInfo.NationalId.ToUpper(),
                    client.Email.ToUpper(), Convert.ToInt32(amount), partnerPaymentSetting.UserName, input.Id);
            }
        }

        public static string CallZippyWebpayV2Api(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                if (string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidEmail);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.ZippyOneClickApiUrl).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var clientName = (!string.IsNullOrEmpty(client.FirstName) || !string.IsNullOrEmpty(client.LastName)) ?
                                  string.Format("{0} {1}", client.FirstName, client.LastName) : client.UserName;
                var amount = input.Amount;
                if (input.CurrencyId != Constants.Currencies.ChileanPeso)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.ChileanPeso);
                    amount = rate * input.Amount;
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.ChileanPeso);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                var paymentRequestInput = new
                {
                    rut = paymentInfo.NationalId,
                    amount = Convert.ToInt32(amount).ToString(),
                    merchantId = partnerPaymentSetting.UserName,
                    merchantReqId = input.Id.ToString(),
                    email = client.Email,
                    name = clientName
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/v2/webpay/generate", url),
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var response = JsonConvert.DeserializeObject<PaymentRequestOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.STATUS.ToLower() == "ok")
                    return response.URL;
                throw new Exception(response.DESCRIPTION);
            }
        }

        public static string CallZippyOneClickApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                if (string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidEmail);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.ZippyOneClickApiUrl).StringValue;
                var privateKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ZippyToken).StringValue;
                var amount = input.Amount;
                if (input.CurrencyId != Constants.Currencies.ChileanPeso)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.ChileanPeso);
                    amount = rate * input.Amount;
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.ChileanPeso);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }

                var payload = new Dictionary<string, string>
                    {
                        { "id",  input.Id.ToString() },
                        { "amount", Convert.ToInt32(input.Amount).ToString() }
                    };
                var token = "";
                RSAParameters rsaParams;
                using (var tr = new StringReader(privateKey))
                {
                    var pemReader = new PemReader(tr);
                    var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                    if (keyPair == null)
                    {
                        throw new Exception("Could not read RSA private key");
                    }
                    var privateRsaParams = keyPair.Private as RsaPrivateCrtKeyParameters;
                    rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
                }
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096))
                {
                    rsa.ImportParameters(rsaParams);
                    token = Jose.JWT.Encode(payload, rsa, JwsAlgorithm.RS256);

                    var paymentRequestInput = new
                    {
                        amount = Convert.ToInt32(amount).ToString(),
                        merchantId = partnerPaymentSetting.UserName,
                        merchantReqId = input.Id.ToString()
                    };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = string.Format("{0}/oneclick/geturlaplication", url),
                        PostData = JsonConvert.SerializeObject(paymentRequestInput)
                    };
                    httpRequestInput.RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + token } };
                    return JsonConvert.DeserializeObject<PaymentRequestOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).URL;
                }
            }
        }

        public static string CallZippyPayInApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                if (string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidEmail);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                if (!CountryCodesByCurrency.ContainsKey(input.CurrencyId))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);

                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ZippyPayInApiUrl).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var clientName = (!string.IsNullOrEmpty(client.FirstName) || !string.IsNullOrEmpty(client.LastName)) ?
                string.Format("{0} {1}", client.FirstName, client.LastName) : client.UserName;
                var returnUrl = string.Format("https://{0}", session.Domain);
                var paymentRequestInput = new
                {
                    merchantId = partnerPaymentSetting.UserName,
                    transactionId = input.Id.ToString(),
                    country = CountryCodesByCurrency[input.CurrencyId],
                    currency = client.CurrencyId,
                    payMethod = PaymentWays[paymentSystem.Name],
                    documentId = paymentInfo.NationalId,
                    amount = input.Amount.ToString("F"),
                    email = client.Email,
                    name = clientName,
                    timestamp = CommonFunctions.GetCurrentUnixTimestampSeconds().ToString(),
                    payinExpirationTime = "240",
                    url_OK = returnUrl,
                    url_ERROR = returnUrl,
                    objData = new { }
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/pay", url),
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var response = JsonConvert.DeserializeObject<PaymentRequestOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.STATUS.ToLower() == "ok")
                    return response.URL;
                throw new Exception(response.DESCRIPTION);
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                if (string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidEmail);
                if (!client.IsEmailVerified)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailNotVerified);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);

                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                if (bankInfo == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.ZippyOneClickApiUrl).StringValue;
                var privateKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ZippyToken).StringValue;
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                if (input.CurrencyId != Constants.Currencies.ChileanPeso)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.ChileanPeso);
                    amount = rate * amount;
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.ChileanPeso);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                var payload = new Dictionary<string, string>
                {
                    { "id",  input.Id.ToString() },
                    { "amount", Convert.ToInt32(amount).ToString("F") }
                };
                var token = string.Empty;
                RSAParameters rsaParams;
                using (var tr = new StringReader(privateKey))
                {
                    var pemReader = new PemReader(tr);
                    var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                    if (keyPair == null)
                        throw new Exception("Could not read RSA private key");
                    var privateRsaParams = keyPair.Private as RsaPrivateCrtKeyParameters;
                    rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
                }
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096))
                {
                    rsa.ImportParameters(rsaParams);
                    token = Jose.JWT.Encode(payload, rsa, JwsAlgorithm.RS256);

                    var clientName = (!string.IsNullOrEmpty(client.FirstName) || !string.IsNullOrEmpty(client.LastName)) ?
                                  string.Format("{0} {1}", client.FirstName, client.LastName) : client.UserName;
                    var paymentRequestInput = new
                    {
                        name = clientName,
                        rut = paymentInfo.NationalId,
                        email = client.Email,
                        amount = Convert.ToInt32(amount).ToString(),
                        merchantId = partnerPaymentSetting.UserName,
                        merchantRequestId = input.Id.ToString(),
                        bankId = bankInfo.BankCode,
                        typeAccountId = Enum.GetName(typeof(PaymentAccountTypes), Convert.ToInt32(paymentInfo.AccountType)),
                        numAccount = paymentInfo.BankAccountNumber
                    };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = string.Format("{0}/cashout/generate", url),
                        PostData = JsonConvert.SerializeObject(paymentRequestInput)
                    };
                    httpRequestInput.RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + token } };
                    var response = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    if (response.CODE == 9)
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding,
                        };
                    throw new Exception(response.MESSAGE);

                }
            }
        }

        public static PaymentResponse CreatePayoutGeneratorRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                if (string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidEmail);
                if (!client.IsEmailVerified)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailNotVerified);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                if (!CountryCodesByCurrency.ContainsKey(input.CurrencyId))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                if (bankInfo == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.ZippyOneClickApiUrl).StringValue;
                var privateKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ZippyToken).StringValue;
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                if (input.CurrencyId != Constants.Currencies.PeruvianSol)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.PeruvianSol);
                    amount = rate * amount;
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.PeruvianSol);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                var payload = new Dictionary<string, string>
                {
                    { "id",  input.Id.ToString() },
                    { "amount", Convert.ToInt32(amount).ToString("F") }
                };
                var token = string.Empty;
                RSAParameters rsaParams;
                using (var tr = new StringReader(privateKey))
                {
                    var pemReader = new PemReader(tr);
                    var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                    if (keyPair == null)
                        throw new Exception("Could not read RSA private key");
                    var privateRsaParams = keyPair.Private as RsaPrivateCrtKeyParameters;
                    rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
                }
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096))
                {
                    rsa.ImportParameters(rsaParams);
                    token = Jose.JWT.Encode(payload, rsa, JwsAlgorithm.RS256);

                    var clientName = (!string.IsNullOrEmpty(client.FirstName) || !string.IsNullOrEmpty(client.LastName)) ?
                                  string.Format("{0} {1}", client.FirstName, client.LastName) : client.UserName;
                    var paymentRequestInput = new
                    {
                        merchantId = partnerPaymentSetting.UserName,
                        transactionId = input.Id.ToString(),
                        country = CountryCodesByCurrency[input.CurrencyId],
                        currency = Constants.Currencies.PeruvianSol,
                        typeDocumentId = paymentInfo.TypeDocumentId,
                        documentId = paymentInfo.NationalId,
                        amount = Convert.ToInt32(amount).ToString("F"),
                        bankId = bankInfo.BankCode,
                        email = client.Email,
                        typeAccountId = Convert.ToInt32(paymentInfo.AccountType).ToString(),
                        phone_number = client.MobileNumber,
                        numAccount = paymentInfo.BankAccountNumber,
                        name = clientName,
                        timestamp = CommonFunctions.GetCurrentUnixTimestampSeconds().ToString(),
                    };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = string.Format("{0}/payOut", url),
                        PostData = JsonConvert.SerializeObject(paymentRequestInput)
                    };
                    httpRequestInput.RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + token } };
                    var response = JsonConvert.DeserializeObject<PayoutGeneratorOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    if (response.Status != "error")
                    {
                        input.ExternalTransactionId = response.TransactionId;
                        paymentSystemBl.ChangePaymentRequestDetails(input);
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding,
                        };
                    }
                    throw new Exception(response.Description);
                }
            }
        }
    }
}