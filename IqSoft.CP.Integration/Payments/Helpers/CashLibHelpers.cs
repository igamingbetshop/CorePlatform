using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.CashLib;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class CashLibHelpers
    {
        public static string CallCashLibApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CashLibApiUrl).StringValue;

                var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                    distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                var redirectUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
                redirectUrl = string.Format("{0}/redirect/RedirectRequest?redirectUrl={1}", redirectUrl, cashierPageUrl);
                var notifyUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var cancelUrl = string.Format(distributionUrlKey.StringValue, session.Domain) +
                                string.Format("/notify/NotifyResult?orderId={0}&returnUrl={1}&domain={2}&providerName=CashLib&methodName=CancelRequest", input.Id, cashierPageUrl, notifyUrl);
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
                var voucherInput = new
                {
                    transaction_id = input.Id.ToString(),
                    mid = partnerPaymentSetting.UserName,
                    purchase_amount = (int)(amount * 100),
                    currency = Constants.Currencies.Euro,
                    ipaddress = session.LoginIp,
                    name = client.Id.ToString(),
                    firstname = client.UserName,
                    birthdate = client.BirthDate.ToString("yyyy-MM-dd"),
                    success_url = redirectUrl,
                    cancel_url = cancelUrl
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "apikey", partnerPaymentSetting.Password } },
                    Url = url,
                    PostData = JsonConvert.SerializeObject(voucherInput)
                };
                var response = JsonConvert.DeserializeObject<VoucherOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                input.ExternalTransactionId = response.TransactionId;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                if (response.Status != 0)
                    return response.ErrorMessage;
                return response.RedirectUrl;
            }
        }
    }
}
