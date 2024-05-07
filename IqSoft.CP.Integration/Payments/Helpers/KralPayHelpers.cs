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
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.KralPay;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class KralPayHelpers
    {
        private static Dictionary<string, string> PaymentServices { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.KralPayPapara, "papara"},
            { Constants.PaymentSystems.KralPayBankTransfer,"banka"},
            { Constants.PaymentSystems.KralPayMefete, "mft" },
            { Constants.PaymentSystems.KralPayCrypto, "crypto"}
        };
        public static string CallKralPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if(client.CurrencyId != Constants.Currencies.TurkishLira)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);

            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.KralPayApiUrl).StringValue;
                var paymenstSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                if (!PaymentServices.ContainsKey(paymenstSystem.Name))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                var bankId = string.Empty;
                if (paymenstSystem.Name == Constants.PaymentSystems.KralPayBankTransfer)
                {
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId)) ??
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                    bankId = bankInfo.BankCode;
                }
                var paymentRequestInput = new
                {
                    sid = partnerPaymentSetting.UserName,
                    username = client.UserName,
                    userID = client.Id,
                    trx = input.Id,
                    return_url = cashierPageUrl,
                    fullname = $"{client.FirstName} {client.LastName}",
                    amount = Math.Round(input.Amount, 2),
                    bankId
                };

                return $"{url}/{PaymentServices[paymenstSystem.Name]}?{CommonFunctions.GetUriEndocingFromObject(paymentRequestInput)}";
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (client.CurrencyId != Constants.Currencies.TurkishLira)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.KralPayPayoutApiUrl).StringValue;
                var paymenstSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                if (!PaymentServices.ContainsKey(paymenstSystem.Name))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var bankId = string.Empty;
                if (paymenstSystem.Name == Constants.PaymentSystems.KralPayBankTransfer)
                {
                    var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId)) ??
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                    bankId = bankInfo.BankCode;
                }

                var payoutRequestInput = new
                {
                    sid = partnerPaymentSetting.UserName,
                    key = partnerPaymentSetting.Password,
                    public_name = "fastyatir",
                    method = PaymentServices[paymenstSystem.Name],
                    username = client.UserName,
                    userID = client.Id,
                    trx = paymentRequest.Id,
                    fullname = $"{client.FirstName} {client.LastName}",
                    amount = Math.Round(paymentRequest.Amount, 2),
                    account = paymentInfo.WalletNumber,
                    trx_code = paymentInfo.WalletNumber,
                    selected_crypto_type = paymentInfo.AccountType,
                    iban = paymentInfo.BankAccountNumber,
                    bank = bankId
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = $"{url}/Withdrawal/{PaymentServices[paymenstSystem.Name]}/",
                    PostData = JsonConvert.SerializeObject(new List<object> { payoutRequestInput })
                };
                var payoutOutput = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (payoutOutput.Code == "200")
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                    };
                throw new Exception($"Code: {payoutOutput.Code}, Description: {payoutOutput.Message}");
            }
        }
    }
}