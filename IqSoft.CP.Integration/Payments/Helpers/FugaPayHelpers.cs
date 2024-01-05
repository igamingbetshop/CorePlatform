using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.FugaPay;
using log4net;
using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class FugaPayHelpers
    {
        public static string CallFugaPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            if (string.IsNullOrEmpty(client.Email) ||
                (paymentSystem.Name == Constants.PaymentSystems.FugaPayCreditCard &&  string.IsNullOrEmpty(client.MobileNumber)))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
            if (input.CurrencyId != Constants.Currencies.TurkishLira)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);

            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.FugaPayApiUrl).StringValue;
                var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var callbackUrl = string.Format("{0}/api/FugaPay/ApiRequest", paymentGatewayUrl);

                var merchantData = partnerPaymentSetting.UserName.Split(',');
                var paymentRequestInput = new PaymentInput
                {
                    OrderID = input.Id.ToString(),
                    ClientUserName = client.Id.ToString(),
                    ClientMail = client.Email,
                    Desc = string.Empty,
                    MerchantCode = merchantData[0],
                    MerchantSecretCode = merchantData[1],
                    MerchantPublicKey = partnerPaymentSetting.Password,
                    SuccessUrl = callbackUrl,
                    FailUrl = callbackUrl,
                    RedirectUrl = cashierPageUrl,
                    Amount = input.Amount
                };
                if (paymentSystem.Name == Constants.PaymentSystems.FugaPayCreditCard)
                {
                    paymentRequestInput.Channel = "minipay";
                    paymentRequestInput.Opt = new OptModel
                    {
                        ClientSenderPhone = client.MobileNumber,
                        TransactionDate = input.CreationTime.ToString("dd.MM.yyyy hh:mm:ss"),
                        IpAddress = session.LoginIp
                    };
                }
                else
                {
                    paymentRequestInput.Channel = "remitUrl";
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId)) ??
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                    paymentRequestInput.BankCode = bankInfo.BankCode;
                }
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    })
                };
                var result = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (result.ResultCode != 0)
                    throw new Exception($"Code: {result.ResultCode}, Description: {result.ResultMessage}, MessageId: {result.MessageID}");
                input.ExternalTransactionId =  result.ResponseUrl.RemitRequestId;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return result.ResponseUrl.Url;
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            if (string.IsNullOrEmpty(client.Email) )
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
            if (paymentRequest.CurrencyId != Constants.Currencies.TurkishLira)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.FugaPayPayoutApiUrl).StringValue;
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);

                var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var callbackUrl = string.Format("{0}/api/FugaPay/ApiRequest", paymentGatewayUrl);

                var merchantData = partnerPaymentSetting.UserName.Split(',');
                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId)) ??
                      throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                var paymentRequestInput = new PaymentInput
                {
                    OrderID = paymentRequest.Id.ToString(),
                    ClientUserName = client.Id.ToString(),
                    ClientMail = client.Email,
                    Desc = string.Empty,
                    MerchantCode = merchantData[0],
                    MerchantSecretCode = merchantData[1],
                    MerchantPublicKey = partnerPaymentSetting.Password,
                    SuccessUrl = callbackUrl,
                    FailUrl = callbackUrl,
                    BankCode = bankInfo.BankCode,
                    Channel = "withdrawRequest",
                    Amount = amount,
                    IbanOwner  = paymentInfo.BankAccountHolder,
                    IbanNumber = paymentInfo.BankAccountNumber
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    })
                };
                var paymentRequestOutput = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (paymentRequestOutput.ResultCode != 0)
                    return new PaymentResponse
                    {
                        Description = $"Code: {paymentRequestOutput.ResultCode}, Description: {paymentRequestOutput.ResultMessage}, MessageId: {paymentRequestOutput.MessageID}",
                        Status = PaymentRequestStates.Failed,
                    };

                paymentRequest.ExternalTransactionId = paymentRequestOutput.Result.WithdrawRequestId;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                if (paymentRequestOutput.Result.Status.ToUpper() == "RJT" || paymentRequestOutput.Result.Status.ToUpper() == "CNL")
                    return new PaymentResponse
                    {
                        Description = paymentRequestOutput.Result.Status,
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