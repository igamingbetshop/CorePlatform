using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.Omid;
using log4net;
using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class OmidHelpers
    {
        public static string CallOmidApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                session.Domain = "embracesoft.com"; // remove
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OmidApiUrl).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var amount = input.Amount - (input.CommissionAmount ?? 0);

                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                if (bankInfo == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                var returnUrl = string.Format("https://{0}", session.Domain);
                var notifyUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var distUrl = string.Format(CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl).StringValue, session.Domain) +
                        string.Format("/Omid/NotifyResult?orderId={0}&returnUrl={1}&domain={2}", input.Id, returnUrl,
                        CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OmidNotifyUrl).StringValue);

                var paymentRequestInput = new
                {
                    mid = partnerPaymentSetting.UserName,
                    amount = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.IranianRial, Convert.ToInt32(amount))),
                    callback = distUrl,
                    bank = bankInfo.BankCode,
                    userid = input.ClientId,
                    transactionid = input.Id
                };
                var httpRequestInput = new HttpRequestInput
                {
                    RequestMethod = System.Net.Http.HttpMethod.Get,
                    Url = string.Format("{0}/trs/webservice/payRequest?params={1}", url, JsonConvert.SerializeObject(paymentRequestInput))
                };
                var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.Error)
                    throw new Exception(response.Message);
                input.ExternalTransactionId = response.Result;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return string.Format("{0}/trs/trs/payment/goToBank/{1}", url, response.Result);
            }
        }           
    }
}