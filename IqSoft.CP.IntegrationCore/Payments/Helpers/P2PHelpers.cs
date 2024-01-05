using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.P2P;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class P2PHelpers
    {
        public static string CallP2PApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.P2PApiUrl).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                if (bankInfo == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                var paymentRequestInput = new
                {
                    username = client.Id,
                    fullName = client.Id,
                    user_id = client.Id,
                    payment_system_id = input.Id,
                    partnerId = partnerPaymentSetting.UserName,
                    accountNumber = paymentInfo.BankAccountNumber,
                    amount = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.IranianTuman, Convert.ToInt32(amount)))
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    RequestHeaders = new Dictionary<string, string> { { "App-Key", partnerPaymentSetting.Password } },
                    Url = string.Format("{0}/deposit", url),
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                input.ExternalTransactionId = response.DepositId;
                paymentSystemBl.ChangePaymentRequestDetails(input);

                return response.Url;
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var partnerBl = new PartnerBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                        input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.P2PApiUrl).StringValue;
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                    if (bankInfo == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                    var amount = input.Amount - (input.CommissionAmount ?? 0);

                    var paymentRequestInput = new
                    {
                        username = client.Id,
                        fullName = client.Id,
                        id = client.Id,
                        payment_system_id = input.Id,
                        partnerId = partnerPaymentSetting.UserName,
                        accountNumber = paymentInfo.BankAccountNumber,
                        amount = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.IranianTuman, Convert.ToInt32(amount)))
                    };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = System.Net.Http.HttpMethod.Post,
                        RequestHeaders = new Dictionary<string, string> { { "App-Key", partnerPaymentSetting.Password } },
                        Url = string.Format("{0}/withdraw ", url),
                        PostData = JsonConvert.SerializeObject(paymentRequestInput)
                    };
                    var response = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    input.ExternalTransactionId = response.WithdrawId;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                    };
                }
            }
        }
    }
}