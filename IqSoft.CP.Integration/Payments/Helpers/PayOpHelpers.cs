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
using System;
using System.Collections.Generic;

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
            { Constants.PaymentSystems.PayOpInstantBankTransfer, "200018" },//Europeans
            { Constants.PaymentSystems.PayOpPayDo, "700001" }
        };

        public static string CallPayOpApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {

            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayOpApiUrl).StringValue;
                var checkoutUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayOpCheckoutUrl).StringValue;
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                if (paymentSystem.Name == Constants.PaymentSystems.PayOpInstantBankTransfer && string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
                if (!PaymentMethods.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
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
                var invoiceInput = new
                {
                    publicKey = partnerPaymentSetting.UserName,
                    order = new
                    {
                        id = input.Id.ToString(),
                        amount = amount.ToString("F"),
                        currency = Constants.Currencies.Euro,
                        items = new List<int>()
                    },
                    payer = new
                    {
                        email = client.Email,
                        name = $"{client.FirstName} {client.LastName}"
                    },
                    language = session.LanguageId,
                    resultUrl = cashierPageUrl,
                    failPath = cashierPageUrl,
                    signature = CommonFunctions.ComputeSha256(string.Format("{0}:{1}:{2}:{3}", amount.ToString("F"),
                                                              Constants.Currencies.Euro, input.Id, partnerPaymentSetting.Password)),
                    paymentMethod = PaymentMethods[paymentSystem.Name]
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
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
}