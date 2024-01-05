using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using IqSoft.CP.Integration.Payments.Models.Impaya;
using IqSoft.CP.Common.Helpers;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class ImpayaHelpers
    { 
        public static string CallImpayaApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                if (string.IsNullOrEmpty(client.FirstName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
                if (string.IsNullOrEmpty(client.LastName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);

                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ImpayaApiUrl).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var amount = Convert.ToInt32(input.Amount*100);
                var paymentInput = new
                {
                    _cmd = "payment_apgp",
                    merchant_id = partnerPaymentSetting.UserName,
                    amount,
                    currency = client.CurrencyId,
                    invoice = input.Id.ToString(),
                    language = CommonHelpers.LanguageISO3Codes.ContainsKey(session.LanguageId) ?
                               CommonHelpers.LanguageISO3Codes[session.LanguageId] : CommonHelpers.LanguageISO3Codes[Constants.DefaultLanguageId],
                    cl_fname = client.FirstName,
                    cl_lname = client.LastName,
                    cl_email = client.Email,
                    cl_country = paymentInfo.Country,
                    cl_city = paymentInfo.City,
                    psys = "",
                    hash = CommonFunctions.ComputeMd5($"{amount}{client.CurrencyId}{partnerPaymentSetting.UserName}{partnerPaymentSetting.Password}")
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpMethod.Post,
                    Url = url,
                    PostData = CommonFunctions.GetUriEndocingFromObject(paymentInput)
                };
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                try
                {
                    var invoiceOutput = JsonConvert.DeserializeObject<InvoiceOutput>(resp);

                    if (invoiceOutput.Response.ToUpper() == "OK" && invoiceOutput.StatusId == 2)
                    {
                        input.ExternalTransactionId =  invoiceOutput.TransactionDetails.TransactionId;
                        paymentSystemBl.ChangePaymentRequestDetails(input);
                        return invoiceOutput._3DS.RedirectUrl;
                    }
                    throw new Exception(invoiceOutput.StatusDescrition);
                }
                catch
                {
                    throw new Exception(resp);
                }
            }
        }
    }
}
