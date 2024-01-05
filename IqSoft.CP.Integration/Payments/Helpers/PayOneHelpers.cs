using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.PayOne;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class PayOneHelpers
    {
        public static string CallPayOneApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBll = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBll))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                    var segment = clientBl.GetClientPaymentSegments(input.ClientId.Value, partnerPaymentSetting.PaymentSystemId).OrderBy(x => x.Priority).FirstOrDefault();

                    var url = segment == null ? CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayOneApiUrl).StringValue : segment.ApiUrl;
                    var uName = segment == null ? partnerPaymentSetting.UserName : segment.ApiKey;
                    var notifyUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;

                    var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                    if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                        distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                    var distUrl = string.Format(distributionUrlKey.StringValue, session.Domain) +
                        string.Format("{0}?id={1}&url={2}&notifyUrl={3}", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayOneNotifyUrl).StringValue, input.Id, cashierPageUrl, notifyUrl);
                    var amount = input.Amount;
                    if (input.CurrencyId != Constants.Currencies.IranianTuman)
                    {
                        var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.IranianTuman, partnerPaymentSetting);
                        amount = rate * input.Amount;
                        var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                        parameters.Add("Currency", Constants.Currencies.IranianTuman);
                        parameters.Add("AppliedRate", rate.ToString("F"));
                        input.Parameters = JsonConvert.SerializeObject(parameters);
                    }

                    var paymentRequestInput = new
                    {
                        MerchantId = uName,
                        Amount = Convert.ToInt32(amount),
                        CallbackUrl = distUrl,
                        OrderId = input.Id,
                        OrderDescription = client.Id
                    };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = string.Format("{0}/api/v1/invoice/request", url),
                        PostData = JsonConvert.SerializeObject(paymentRequestInput)
                    };
                    var response = JsonConvert.DeserializeObject<PaymentRequestOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    if (response.Status == 1)
                    {
                        input.ExternalTransactionId = response.InvoiceId;
                        paymentSystemBll.ChangePaymentRequestDetails(input);
                        return string.Format("{0}/invoice/pay/{1}", url, response.InvoiceId);
                    }
                    paymentSystemBll.ChangePaymentRequestDetails(input);
                    throw new Exception(response.Description);
                }
            }
        }
    }
}
