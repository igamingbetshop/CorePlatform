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

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class WebPaysHelpers
    {
        private static Dictionary<string, string> PaymentServices { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.WebPaysCreditCard, "CC"},
            { Constants.PaymentSystems.WebPaysAPMs,"APMs"}
        };

        public static string CallWebPaysApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (string.IsNullOrEmpty(client.MobileNumber))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
            if (string.IsNullOrEmpty(client.Address))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);

            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            if (!PaymentServices.ContainsKey(paymentSystem.Name))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
            var paymentRequestInput = new
            {
                public_key = partnerPaymentSetting.Password,
                terNO = partnerPaymentSetting.UserName,
                retrycount = 5,
                unique_reference = "Y",
                source_url = $"https://{session.Domain}",
                bill_amt = Math.Round(input.Amount, 2),
                bill_currency = client.CurrencyId,
                product_name = session.Domain,
                mop = PaymentServices[paymentSystem.Name],
                fullname = $"{client.FirstName} {client.LastName}",
                bill_email = client.Email,
                bill_address = client.Address,
                bill_city = paymentInfo.City,
                bill_state = paymentInfo.City,
                bill_country = paymentInfo.Country,
                bill_zip = string.IsNullOrEmpty(client.ZipCode.Trim()) ? "dummy" : client.ZipCode.Trim(),
                bill_phone = client.MobileNumber,
                reference = input.Id,
                webhook_url =$"{paymentGatewayUrl}/api/WebPays/ApiRequest",
                return_url = cashierPageUrl
            };

            var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
            if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

            var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
            return $"{distributionUrl}/webpays/paymentprocessing?data={AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(paymentRequestInput))}";
           
        }
    }
}
