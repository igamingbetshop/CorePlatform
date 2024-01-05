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
using System.Collections.Generic;
using System.Text;
using IqSoft.CP.Integration.Payments.Models.CoinsPaid;
using IqSoft.CP.Integration.Payments.Models;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class CoinsPaidHelpers
    {
        public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, 
                                                                               (int)PaymentRequestTypes.Deposit);
            var id = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CoinsPaidId).NumericValue;
            var postData = JsonConvert.SerializeObject(new
            {
                client_id = id,
                currency = paymentInfo.AccountType,
                convert_to = client.CurrencyId,
                foreign_id = input.Id.ToString(),
                url_back = cashierPageUrl
            });
            var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(postData));
            var signature = CommonFunctions.ComputeHMACSha512(postData, partnerPaymentSetting.Password).ToLower();
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CoinsPaidTerminalUrl).StringValue;
           return string.Format(url, data, signature);
        }
         

        public static PaymentResponse PayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.CoinsPaid);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                var postData = new
                {
                    foreign_id = input.Id.ToString(),
                    amount,
                    currency = client.CurrencyId,
                    convert_to = paymentInfo.AccountType,
                    address = paymentInfo.WalletNumber
                };
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CoinsPaidUrl).StringValue;
                var json = JsonConvert.SerializeObject(postData);
                var signature = CommonFunctions.ComputeHMACSha512(json, partnerPaymentSetting.Password).ToLower();

                var requestHeaders = new Dictionary<string, string>
                {
                   { "X-Processing-Key", partnerPaymentSetting.UserName},
                   { "X-Processing-Signature", signature}
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    Url = String.Format(url, "withdrawal/crypto"),
                    RequestHeaders = requestHeaders,
                    PostData = JsonConvert.SerializeObject(postData)
                };
                var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var output = JsonConvert.DeserializeObject<PayoutOutput>(response);
                input.ExternalTransactionId = output.Data.Id.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(input);
                
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
        }
    }
}

