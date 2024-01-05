using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.PayOp;
using log4net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class PayOpHelpers
    {
        public static Dictionary<string, string> PaymentMethods { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.PayOpPIX, "200969" }, //Brazil
            { Constants.PaymentSystems.PayOpNeosurf, "200008" }, //Europeans
            { Constants.PaymentSystems.PayOpRevolut, "3822" },//Europeans
            { Constants.PaymentSystems.PayOpLocalBankTransfer, "704" },//for Latam via Safetypay
            { Constants.PaymentSystems.PayOpInstantBankTransfer, "200018" }//Europeans
        };

        public static string CallPayOpApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using var paymentSystemBl = new PaymentSystemBll(session, log);
            var client = CacheManager.GetClientById(input.ClientId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayOpApiUrl).StringValue;
            var checkoutUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayOpCheckoutUrl).StringValue;
            var notifyUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            if (!PaymentMethods.ContainsKey(paymentSystem.Name))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
            var invoiceInput = new
            {
                publicKey = partnerPaymentSetting.UserName,
                order = new
                {
                    id = input.Id.ToString(),
                    amount = input.Amount.ToString("F"),
                    currency = client.CurrencyId,
                    items = new List<int>()
                },
                payer = new
                {
                    email = client.Email,
                    name = client.UserName
                },
                language = session.LanguageId,
                resultUrl = cashierPageUrl,
                failPath = cashierPageUrl,
                signature = CommonFunctions.ComputeSha256(string.Format("{0}:{1}:{2}:{3}", input.Amount.ToString("F"),
                                                          client.CurrencyId, input.Id, partnerPaymentSetting.Password)),
                paymentMethod = PaymentMethods[paymentSystem.Name]
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = HttpMethod.Post,
                Url = string.Format("{0}invoices/create", url),
                PostData = JsonConvert.SerializeObject(invoiceInput)
            };
            var response = JsonConvert.DeserializeObject<InvoiceOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (response.Status != 1)
                return response.Message;
            return string.Format(checkoutUrl, session.LanguageId, response.Data);
        }
    }
}