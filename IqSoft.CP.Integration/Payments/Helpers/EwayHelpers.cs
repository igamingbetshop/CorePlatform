using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using IqSoft.CP.Integration.Payments.Models.Eway;
using System.Collections.Generic;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class EwayHelpers
    {
        public static string CallEwayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);              
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.EwayApiUrl).StringValue;
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var paymentGatewayUrl = string.Format("{0}/api/Eway/ApiRequest", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue);
                var redirectData = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(new
                {
                    PaymentGatewayUrl = paymentGatewayUrl,
                    CashierPageUrl = cashierPageUrl                    
                }));
                var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                    distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
                var redirectUrl = string.Format("{0}/redirect/rp?rd={1}&transactionId={2}", distributionUrl, redirectData, input.Id);
                var paymentInput = JsonConvert.SerializeObject(new
                {
                    Payment = new
                    {
                        TotalAmount = (int)(input.Amount * 100),
                        InvoiceReference = input.Id,
                        CurrencyCode = client.CurrencyId
                    },
                    CustomerIP = paymentInfo.TransactionIp,
                    Method = "ProcessPayment",
                    TransactionType = "Purchase",
                    RedirectUrl = redirectUrl,
                    CancelUrl = redirectUrl
                });
                var byteArray = Encoding.Default.GetBytes($"{partnerPaymentSetting.Password}:{partnerPaymentSetting.UserName}");
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(byteArray) } },                
                    Url = string.Format("{0}/AccessCodesShared", url),
                    PostData = paymentInput
                };
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (!string.IsNullOrEmpty(paymentOutput.Errors))
                    throw new Exception(paymentOutput.Errors);
                var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                     JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                parameters.Add("AccessCode", paymentOutput.AccessCode);
                input.Parameters = JsonConvert.SerializeObject(parameters);
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return paymentOutput.SharedPaymentUrl;
            }
        }
    }
}
