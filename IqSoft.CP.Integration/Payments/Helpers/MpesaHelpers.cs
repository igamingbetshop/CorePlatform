using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using IqSoft.CP.Integration.Payments.Models.Mpesa;
using System.Text;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class MpesaHelpers
    {
        private readonly static List<string> TransactionTypes = new List<string>
        {
           "IF",  //InternalFundsTransfer
           "RT",  //RTGS
           "PL",  //PESALINK
           "EF",  //EFT
           "MO" //MOBILE MONEY
        };

        public static string CallMpesaApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               client.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.MpesaApiUrl);
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var callbackUrl = string.Format("{0}/api/Mpesa/ApiRequest", paymentGatewayUrl);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            if (string.IsNullOrEmpty(paymentInfo.MobileNumber))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
            var paymentRequestInput = new
            {
                phoneNumber = paymentInfo.MobileNumber,
                amount = input.Amount.ToString(),
                invoiceNumber = $"KCBTILLNO-{input.Id}",
                sharedShortCode = true,
                orgShortCode = "",
                orgPassKey = "",
                transactionDescription = "Deposit",
                callbackUrl
            };
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + GenerateToken(client.PartnerId, $"{partnerPaymentSetting.UserName}:{partnerPaymentSetting.Password}") },
                { "operation", "STKPush"},
                { "routeCode", "207"},
                { "messageId", "232323_KCBOrg_8875661561"}
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = headers,
                Url = $"{url}/mm/api/request/1.0.0/stkpush",
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var result = JsonConvert.DeserializeObject<PaymentOutput>(response);
            if (result?.Response?.ResponseCode != 0)
                throw new Exception(response);
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                input.ExternalTransactionId =  result?.Response.MerchantRequestID;
                paymentSystemBl.ChangePaymentRequestDetails(input);
            }
            return result?.Response.CustomerMessage;
        }

        private static string GenerateToken(int partnerId, string usernamePassword)
        {
            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Mpesa);
            var url = CacheManager.GetPartnerPaymentSystemByKey(partnerId, paymentSystem.Id, Constants.PartnerKeys.MpesaApiUrl);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(usernamePassword)) } },
                Url = $"{url}/token?grant_type=client_credentials",
            };
            return JsonConvert.DeserializeObject<TokenOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).AccessToken;
        }

        private static string GenerateDirectToken(int partnerId, string usernamePassword)
        {
            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.MpesaB2C);
            var url = CacheManager.GetPartnerPaymentSystemByKey(partnerId, paymentSystem.Id, Constants.PartnerKeys.MpesaDirectApiUrl);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Get,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(usernamePassword)) } },
                Url = $"{url}/oauth/v1/generate?grant_type=client_credentials",
            };
            return JsonConvert.DeserializeObject<TokenOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).AccessToken;
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log) // template 15
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                      paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MpesaApiUrl).StringValue;
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId)) ??
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                if (string.IsNullOrEmpty(paymentInfo.AccountType) || !TransactionTypes.Contains(paymentInfo.AccountType))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AccountTypeNotFound);
                var partnerAccount = partnerPaymentSetting.UserName.Split(',');
                var paymentRequestInput = new
                {
                    beneficiaryDetails = paymentInfo.NationalId,
                    companyCode = partnerAccount[1],
                    creditAccountNumber = partnerAccount[0],
                    currency = client.CurrencyId,
                    debitAccountNumber = paymentInfo.BankAccountNumber,
                    debitAmount = amount,
                    paymentDetails = "UT Fund withdrawal",
                    transactionReference = paymentRequest.Id.ToString(),
                    transactionType = paymentInfo.AccountType,
                    beneficiaryBankCode = bankInfo.BankCode
                };
                var headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer " + GenerateToken(client.PartnerId, partnerPaymentSetting.Password) }
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = headers,
                    Url = $"{url}/fundstransfer/1.0.0/api/v1/transfer",
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var payoutOutput = JsonConvert.DeserializeObject<PayoutOutput>(resp);
                if (!string.IsNullOrEmpty(payoutOutput?.MerchantID))
                {
                    paymentRequest.ExternalTransactionId = payoutOutput.MerchantID;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                }
                if (payoutOutput.StatusCode != "0")
                    throw new Exception(resp);
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
        }

        public static PaymentResponse CreateB2CPayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)  //Withdraw template 4
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.MpesaDirectApiUrl);
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var callbackUrl = string.Format("{0}/api/MpesaB2C/PaymentRequest", paymentGatewayUrl);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            if (string.IsNullOrEmpty(paymentInfo.MobileNumber))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
            var amount = input.Amount - (input.CommissionAmount ?? 0);
            var credentials = partnerPaymentSetting.UserName.Split(',');
            if (credentials.Length != 2)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongApiCredentials);

            var securityCredential = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.MpesaKey);
            var paymentRequestInput = new
            {
                OriginatorConversationID = input.Id.ToString(),
                InitiatorName = credentials[1],
                SecurityCredential = securityCredential,
                CommandID = "BusinessPayment",
                Amount = amount,
                PartyA = Convert.ToInt32(credentials[0]),
                PartyB = paymentInfo.MobileNumber,
                Remarks = client.UserName,
                QueueTimeOutURL = callbackUrl,
                ResultURL = callbackUrl,
                Occassion = string.Empty
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + GenerateDirectToken(client.PartnerId, $"{partnerPaymentSetting.Password}") } },
                Url = $"{url}/mpesa/b2c/v3/paymentrequest",
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var result = JsonConvert.DeserializeObject<DirectPaymentOutput>(response);
            if (result?.ResponseCode != "0")
                throw new Exception(response);
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                input.ExternalTransactionId =  result?.ConversationID;
                paymentSystemBl.ChangePaymentRequestDetails(input);
            }
            return new PaymentResponse
            {
                Status = PaymentRequestStates.PayPanding,
            };
        }

        public static void RegisterUrls(int partnerId)
        {
            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.MpesaC2B);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, paymentSystem.Id, "KES", (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerPaymentSystemByKey(partnerId, paymentSystem.Id, Constants.PartnerKeys.MpesaDirectApiUrl);

            var httpInput = new 
            {
                ShortCode = partnerPaymentSetting.UserName,
                ResponseType = "Cancelled",
                ValidationURL = "https://paymentgatewaytest.craftbetstage.com/api/C2B/1/ValidationRequest",
                ConfirmationURL= "https://paymentgatewaytest.craftbetstage.com/api/C2B/1/PayBillRequest"
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + GenerateDirectToken(partnerId, $"{partnerPaymentSetting.Password}") } },
                Url = $"{url}/mpesa/c2b/v2/registerurl",
                PostData = JsonConvert.SerializeObject(httpInput)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
    }
}