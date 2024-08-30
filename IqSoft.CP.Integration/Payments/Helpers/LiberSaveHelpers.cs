using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models.LiberSave;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class LiberSaveHelpers
    {
        public static string CallLiberSaveApi(PaymentRequest input, string cashierPageUrl)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.LiberSaveApiUrl);

            var paymentInput = new
            {
                amount = input.Amount,
                order_id = input.Id.ToString(),
                currency = client.CurrencyId,
                email = client.Email,
                redirect_url = cashierPageUrl
            };                

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "x-api-key", partnerPaymentSetting.UserName } },
                Url = url,
                PostData = JsonConvert.SerializeObject(paymentInput)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
            if (!string.IsNullOrEmpty(output.RedirectUrl))
                return output.RedirectUrl;
            throw new Exception($"Error: {response}");
        }
    }
}
