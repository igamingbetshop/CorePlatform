﻿using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using log4net;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class CryptonPayHelpers
    {
        public static string CallCryptonPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.MobileNumber) || string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);

            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var resourcesUrlKey = "http://10.50.17.10:10000/"; // CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResourcesUrl);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
            if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);

            var paymentProcessingInput = new
            {
                OrderId = input.Id,
                RedirectUrl = cashierPageUrl,
                ResponseUrl = string.Format("{0}/api/CryptonPay/ProcessPaymentRequest", paymentGatewayUrl),
                CancelUrl = string.Format("{0}/api/Payment/CancelPaymentRequest", paymentGatewayUrl),
                input.Amount,
                Currency = input.CurrencyId,
                BillingAddress = client.Address?.Trim(),
                HolderName = string.Format("{0} {1}", client.FirstName, client.LastName),
                PartnerDomain = "10.50.17.10:10000", // session.Domain,
                ResourcesUrl = resourcesUrlKey, //(resourcesUrlKey == null || resourcesUrlKey.Id == 0 ? ("https://resources." + session.Domain) : resourcesUrlKey.StringValue),
                session.LanguageId,
                CountryCode = paymentInfo.Country,
                ZipCode = client.ZipCode?.Trim(),
                paymentInfo.City,
                client.PartnerId,
                PaymentSystemName = input.PaymentSystemName.ToLower()
            };
            var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
            if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

            var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
            var data = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(paymentProcessingInput));
            return string.Format("{0}/paymentform/paymentprocessing?data={1}", distributionUrl, data);
        }
    }
}