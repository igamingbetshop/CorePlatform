using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using log4net;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class IPSHelpers
    {
        public static string CallIPSApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                parameters.Add("Domain", session.Domain );
                input.Parameters = JsonConvert.SerializeObject(parameters);
                paymentSystemBl.ChangePaymentRequestDetails(input);
                if (string.IsNullOrEmpty(client.FirstName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
                if (string.IsNullOrEmpty(client.LastName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
                if (string.IsNullOrWhiteSpace(client.ZipCode))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
                if (string.IsNullOrWhiteSpace(client.Address))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
                if (string.IsNullOrEmpty(client.MobileNumber) || string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
                var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var resourcesUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResourcesUrl);

                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
                if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                var paymentProcessingInput = new
                {
                    OrderId = input.Id,
                    RedirectUrl = cashierPageUrl,
                    ResponseUrl = string.Format("{0}/api/IPS/ProcessPaymentRequest", paymentGatewayUrl),
                    CancelUrl = string.Format("{0}/api/Payment/CancelPaymentRequest", paymentGatewayUrl),
                    input.Amount,
                    Currency = input.CurrencyId,
                    BillingAddress = client.Address?.Trim(),
                    HolderName = string.Format("{0} {1}", client.FirstName, client.LastName),
                    PartnerDomain = session.Domain,
                    ResourcesUrl = (resourcesUrlKey == null || resourcesUrlKey.Id == 0 ? ("https://resources." + session.Domain) : resourcesUrlKey.StringValue),
                    session.LanguageId,
                    CountryCode = paymentInfo.Country,
                    ZipCode = client.ZipCode?.Trim(),
                    paymentInfo.City,
                    client.PartnerId,
                    PaymentSystemName = input.PaymentSystemName.ToLower()
                };
                var distributionUrl = string.Format(CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl).StringValue, session.Domain);
                var data = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(paymentProcessingInput));
                return string.Format("{0}/paymentform/paymentprocessing?data={1}", distributionUrl, data);
            }
        }
    }
}