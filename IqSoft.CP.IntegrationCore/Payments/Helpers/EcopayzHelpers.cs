using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using EcopayzServiceReferenceProd;
using IqSoft.CP.Integration.Payments.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class EcopayzHelpers
    {
        public static string CallEcopayzApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.EcopayzApiUrl).StringValue;
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var merchant = partnerPaymentSetting.UserName.Split(',');
                if (merchant.Length != 2)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var callbackUrl = string.Format("{0}/api/EcoPayz/ApiRequest", paymentGateway);
                var transferUrl = string.Format("{0}/api/EcoPayz/Notify", paymentGateway);

                var paymentRequestInput = new
                {
                    PaymentPageID = merchant[0], //MerchantId
                    MerchantAccountNumber = merchant[1],
                    CustomerIdAtMerchant = client.Id,
                    TxID = input.Id.ToString(),
                    Amount = amount,
                    Currency = input.CurrencyId,
                    MerchantFreeText = partner.Name,
                    OnSuccessUrl = cashierPageUrl,
                    OnFailureUrl = cashierPageUrl,
                    TransferUrl = transferUrl,
                    CallbackUrl = callbackUrl,
                    FirstName = client.Id,
                    LastName = client.UserName
                };
                var inputData = CommonFunctions.GetUriDataFromObject(paymentRequestInput);
                var checksum = paymentRequestInput.GetType().GetProperties().Aggregate(string.Empty, (current, par) => current + par.GetValue(paymentRequestInput, null));
                log.Info("CheckSum: " + checksum);
                return string.Format("{0}/PrivateArea/WithdrawOnlineTransfer.aspx?{1}&Checksum={2}",
                                      url, inputData, CommonFunctions.ComputeMd5(checksum + partnerPaymentSetting.Password));
            }
        }

        public static string PayVoucher(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.EcopayzApiUrl).StringValue;
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var merchant = partnerPaymentSetting.UserName.Split(',');
                if (merchant.Length != 2)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var callbackUrl = string.Format("{0}/api/EcoPayz/ApiRequest", paymentGateway);
                var transferUrl = string.Format("{0}/api/EcoPayz/PayVoucher", paymentGateway);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                var paymentRequestInput = new
                {
                    PaymentPageID = merchant[0], //MerchantId
                    MerchantAccountNumber = merchant[1],
                    CustomerIdAtMerchant = client.Id,
                    TxID = input.Id.ToString(),
                    Currency = partnerPaymentSetting.CurrencyId,
                    MerchantFreeText = partner.Name,
                    OnSuccessUrl = cashierPageUrl,
                    OnFailureUrl = cashierPageUrl,
                    TransferUrl = transferUrl,
                    CancellationUrl = cashierPageUrl,
                    CallbackUrl = callbackUrl,
                    VoucherAmount = amount,
                    VoucherCurrency = client.CurrencyId,
                    FirstName = client.Id,
                    LastName = client.UserName
                };

                var inputData = CommonFunctions.GetUriDataFromObject(paymentRequestInput);
                var checksum = paymentRequestInput.GetType().GetProperties().Aggregate(string.Empty, (current, par) => current + par.GetValue(paymentRequestInput, null));
                log.Info("CheckSum: " + checksum);
                return string.Format("{0}/PrivateArea/ecoVoucherOnlineTransfer.aspx?{1}&Checksum={2}",
                                      url, inputData, CommonFunctions.ComputeMd5(checksum + partnerPaymentSetting.Password));
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                    input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var merchant = partnerPaymentSetting.UserName.Split(',');
                if (merchant.Length != 2)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                var payoutInput = new PayoutRequestType
                {
                    MerchantID = Convert.ToInt32(merchant[0]),
                    MerchantPassword = partnerPaymentSetting.Password,
                    MerchantAccountNumber = Convert.ToInt32(merchant[1]),
                    ClientAccountNumber = Convert.ToInt32(paymentInfo.WalletNumber),
                    TxID = input.Id.ToString(),
                    Currency = (CurrencyType)Enum.Parse(typeof(CurrencyType), client.CurrencyId),
                    Amount = Convert.ToInt32(amount * 100)
                };
                var config = new MerchantAPIServiceSoapClient.EndpointConfiguration();

                var merchantAPIServiceSoapClient = new MerchantAPIServiceSoapClient(config);
                var res = merchantAPIServiceSoapClient.PayoutAsync(payoutInput).Result.Body.TransactionResponse;
                if (res.ErrorCode == 0)
                {
                    input.ExternalTransactionId = res.SVSTxID.ToString();
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Approved,
                    };
                }
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Failed,
                    Description = res.Message
                };
            }
        }
    }
}