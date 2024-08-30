using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class ExternalCashierHelpers
    {
        public static string CallExternalCashierApi(PaymentRequest input, string cashierPageUrl, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var redirectUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ExternalCashierRedirectUrl).StringValue;
            var apiUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ExternalCashierApiUrl).StringValue;
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            if (!string.IsNullOrEmpty(apiUrl))
            {
                var httpRequestInput = new HttpRequestInput
                {
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestHeaders = new Dictionary<string, string> { { "x-api-key", partnerPaymentSetting.UserName } },
                    Url = apiUrl,
                    PostData =JsonConvert.SerializeObject(new { user = client.Id.ToString(), OrderId = input.Id.ToString(), amount = input.Amount })
                };
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                log.Debug(resp);
            }
            return $"{redirectUrl}/{client.Id}?returnUrl={Uri.EscapeDataString(cashierPageUrl)}";
        }
    }
}