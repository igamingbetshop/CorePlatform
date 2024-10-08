using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models.LiberSave;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class LiberSaveHelpers
    {
        private static Dictionary<string, KeyValuePair<string, string>> PaymentServices { get; set; } = new Dictionary<string, KeyValuePair<string, string>>
        {
            { Constants.PaymentSystems.LiberSaveRevolutGB, new KeyValuePair<string, string> ("revolut","GB") },
            { Constants.PaymentSystems.LiberSaveRevolutEU, new KeyValuePair<string, string> ("revolut_eu", string.Empty) },
            { Constants.PaymentSystems.LiberSaveMonzoGB, new KeyValuePair<string, string> ("monzo_ob", "GB") },
            { Constants.PaymentSystems.LiberSavePayPalGB, new KeyValuePair<string, string> ("paypal", "GB") },
            { Constants.PaymentSystems.LiberSaveSepaEU, new KeyValuePair<string, string> ("wise-live", string.Empty) }
        };

        public static string CallLiberSaveApi(PaymentRequest input, SessionIdentity sessionIdentity, string cashierPageUrl)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.LiberSaveApiUrl);
            var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            string paymentInput;
            if (paymentSystem.Name == Constants.PaymentSystems.LiberSave)
            {
                paymentInput = JsonConvert.SerializeObject(new
                {
                    amount = input.Amount,
                    order_id = input.Id.ToString(),
                    currency = client.CurrencyId,
                    email = client.Email,
                    redirect_url = cashierPageUrl
                });
            }
            else
            {
                if (!PaymentServices.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(sessionIdentity.LanguageId, Constants.Errors.PaymentSystemNotFound);

                paymentInput = JsonConvert.SerializeObject(new
                {
                    bank_id = PaymentServices[paymentSystem.Name].Key,
                    bank_country_code = !string.IsNullOrEmpty(PaymentServices[paymentSystem.Name].Value) ? PaymentServices[paymentSystem.Name].Value : sessionIdentity.Country,
                    amount = input.Amount,
                    order_id = input.Id.ToString(),
                    currency = client.CurrencyId,
                    email = client.Email,
                    redirect_url = cashierPageUrl
                });
            }
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "x-api-key", partnerPaymentSetting.UserName } },
                Url = url,
                PostData = paymentInput
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
            if (!string.IsNullOrEmpty(output.RedirectUrl))
                return output.RedirectUrl;
            throw new Exception($"Error: {response}");
        }
    }
}