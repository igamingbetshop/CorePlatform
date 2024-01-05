using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Qaicash;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class QaicashHelpers
    {
        private static Dictionary<string, string> PaymentMethods { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.QaicashBankTransfer, "BANK_TRANSFER" },
            { Constants.PaymentSystems.QaicashJPay, "BANK_TRANSFER" },
            { Constants.PaymentSystems.QaicashBankTransferOnline, "LBT_ONLINE" },
            { Constants.PaymentSystems.QaicashQR, "THB_QR" },
            { Constants.PaymentSystems.QaicashDirect, "DIRECT_PAYMENT" },
            { Constants.PaymentSystems.QaicashOnRampEWallet, "ONRAMP_EWALLET" }
        };

        private static Dictionary<string, string> PayoutMethods { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.QaicashBankTransfer, "LOCAL_BANK_TRANSFER" },
            { Constants.PaymentSystems.QaicashNissinPay, "ID_BANK_TRANSFER" },
            { Constants.PaymentSystems.QaicashJPay, "ID_BANK_TRANSFER" },
            { Constants.PaymentSystems.QaicashOnRampEWallet, "ONRAMP_EWALLET" }
        };
        private static readonly string APIVersion = "v2.0";

        public static string CallQaicashApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.QaicashApiUrl).StringValue;
            var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            var currentTime = DateTime.Now.ToString("yyyyMMddTHHmmssK");
            var amount = Math.Round(input.Amount, 2);
            var depositMethod = PaymentMethods[paymentSystem.Name];
            var messageAuthenticationCode = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}", partnerPaymentSetting.UserName,
                                     input.Id, amount, input.CurrencyId, currentTime, input.ClientId, depositMethod);
            messageAuthenticationCode = CommonFunctions.ComputeHMACSha256(messageAuthenticationCode, partnerPaymentSetting.Password).ToLower();
            var channel = string.Empty;
            if (paymentSystem.Name == Constants.PaymentSystems.QaicashNissinPay)
                channel = "NISSINPAY";
            else if (paymentSystem.Name == Constants.PaymentSystems.QaicashJPay)
                channel = "JPAY";
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            var requestInput = new
            {
                apiVersion = APIVersion,
                merchantId = partnerPaymentSetting.UserName,
                depositorName = paymentInfo.Info,
                amount,
                orderId = input.Id,
                currency = input.CurrencyId,
                dateTime = currentTime,
                language = CommonHelpers.LanguageISO5646Codes.ContainsKey(session.LanguageId) ? CommonHelpers.LanguageISO5646Codes[session.LanguageId] : "en-Us",
                depositorUserId = input.ClientId.ToString(),
                depositMethod,
                redirectUrl = cashierPageUrl,
                callbackUrl = string.Format("{0}/api/Qaicash/ApiRequest", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue),
                messageAuthenticationCode,
                bankingCurrency = paymentSystem.Name == Constants.PaymentSystems.QaicashBankTransfer ? Constants.Currencies.JapaneseYen : string.Empty,
                selectedProvider = channel
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = string.Format("{0}/ago/integration/{1}/{2}/deposit", url, APIVersion, partnerPaymentSetting.UserName),
                PostData = CommonFunctions.GetUriEndocingFromObject(requestInput)
            };
            var resp = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (resp.Success)
            {
                using (var paymentSystemBl = new PaymentSystemBll(session, log))
                {
                    input.ExternalTransactionId = resp.DepositTransaction.TransactionId;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    return resp.PaymentPageSession.PaymentPageUrl;
                }
            }
            throw new Exception(resp.Message);
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                if (string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty); 
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                    paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.QaicashApiUrl).StringValue;
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var currentTime = DateTime.Now.ToString("yyyyMMddTHHmmssK");
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                if (bankInfo == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                var messageAuthenticationCode = string.Empty;
                if (paymentSystem.Name == Constants.PaymentSystems.QaicashNissinPay)
                    messageAuthenticationCode = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", partnerPaymentSetting.UserName,
                                          paymentRequest.Id, amount, paymentRequest.CurrencyId, currentTime, paymentRequest.ClientId);
                else
                    messageAuthenticationCode = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", partnerPaymentSetting.UserName,
                                           paymentRequest.Id, amount, paymentRequest.CurrencyId, currentTime, paymentRequest.ClientId,
                                           paymentInfo.NationalId, bankInfo.BankCode, paymentInfo.BankAccountNumber);
                messageAuthenticationCode = CommonFunctions.ComputeHMACSha256(messageAuthenticationCode, partnerPaymentSetting.Password).ToLower();
                var channel = string.Empty;
                if (paymentSystem.Name == Constants.PaymentSystems.QaicashNissinPay)
                    channel = "NISSINPAY";
                else if (paymentSystem.Name == Constants.PaymentSystems.QaicashJPay)
                    channel = "JPAY";
                var payoutRequestInput = new
                {
                    apiVersion = APIVersion,
                    merchantId = partnerPaymentSetting.UserName,
                    amount,
                    orderId = paymentRequest.Id,
                    currency = paymentRequest.CurrencyId,
                    depositorName = paymentInfo.NationalId,
                    payoutMethod = PayoutMethods[paymentSystem.Name],
                    dateTime = currentTime,
                    language = CommonHelpers.LanguageISO5646Codes.ContainsKey(session.LanguageId) ? CommonHelpers.LanguageISO5646Codes[session.LanguageId] : "en-Us",
                    userId = paymentRequest.ClientId,
                    redirectUrl = string.Format("https://{0}", session.Domain),
                    callbackUrl = string.Format("{0}/api/Qaicash/PayoutRequest", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue),
                    bank = bankInfo.BankCode,
                    accountNumber = paymentInfo.BankAccountNumber,
                    beneficiaryName = paymentInfo.NationalId,
                    branch = paymentInfo.BankBranchName,
                    branchNumber = paymentInfo.BankIBAN,
                    withdrawerEmail = client.Email,
                    messageAuthenticationCode,
                    bankingCurrency = paymentSystem.Name == Constants.PaymentSystems.QaicashNissinPay ? Constants.Currencies.JapaneseYen : string.Empty,
                    selectedProvider = channel,
                    preferredProvider = channel
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/ago/integration/{1}/{2}/payout/preapproved", url, payoutRequestInput.apiVersion, partnerPaymentSetting.UserName),
                    PostData = CommonFunctions.GetUriEndocingFromObject(payoutRequestInput)
                };
                var paymentRequestOutput = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (paymentRequestOutput.Status.ToLower() == "approved")
                {
                    paymentInfo.BeneficiaryName = paymentInfo.NationalId;
                    paymentRequest.Info = JsonConvert.SerializeObject(paymentInfo.Info);
                    paymentRequest.ExternalTransactionId = paymentRequestOutput.TransactionId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                    };
                }
                throw new Exception(string.Format("Code: {0}, Error: {1}", paymentRequestOutput.Status, paymentRequestOutput.Notes));
            }
        }

        public static string GetMerchantBanks(int partnerId)
        {
            var url = "https://public-services.yukon-100.com"; // CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.QaicashApiUrl).StringValue;
            var requestInput = new
            {
                apiVersion = "v2.0",
                merchantId = 110,
                hmac = CommonFunctions.ComputeHMACSha256("110", "qvFYCEIgia136GUVM2rq").ToLower()
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Get,
                Url = string.Format("{0}/ago/integration/{1}/110/payout/routing/methods/complete?{2}", url, requestInput.apiVersion, 
                CommonFunctions.GetUriDataFromObject(requestInput))
            };
            return CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

    }
}
