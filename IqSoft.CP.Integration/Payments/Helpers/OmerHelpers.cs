using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Integration.Payments.Models.OmerPay;
using System.Text.RegularExpressions;
using IqSoft.CP.Integration.Payments.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Net.WebRequestMethods;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class OmerHelpers
    {
        public static string CallOmerApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);

                #region Check
                if (string.IsNullOrEmpty(client.FirstName?.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
                if (string.IsNullOrEmpty(client.LastName?.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
                if (string.IsNullOrEmpty(client.Address)) 
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
                if (string.IsNullOrEmpty(client.RegionId.ToString()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                if (string.IsNullOrEmpty(client.ZipCode?.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
                if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                if (string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
                if (string.IsNullOrEmpty(client.MobileNumber))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
                if (string.IsNullOrEmpty(paymentInfo.TransactionIp.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.NotAllowed);
                if (string.IsNullOrEmpty(client.CurrencyId))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);
                if (client.RegionId == 0)
                    client.RegionId = Constants.DefaultRegionId;
                #endregion

                var region = CacheManager.GetRegionById(client.RegionId, session.LanguageId);
                var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var resourcesUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResourcesUrl);
                var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                              JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);

                parameters.Add("Domain", session.Domain);
                var amount = input.Amount;
                if (input.CurrencyId != Constants.Currencies.USADollar)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.USADollar);
                    amount = Math.Round(rate * input.Amount, 2);
                    parameters.Add("Currency", Constants.Currencies.USADollar);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                }
                input.Parameters = JsonConvert.SerializeObject(parameters);
                paymentSystemBl.ChangePaymentRequestDetails(input);
                var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                    distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);
             
                var paymentProcessingInput = new
                {
                    OrderId = input.Id,
                    RedirectUrl = cashierPageUrl,
                    ResponseUrl = string.Format("{0}/api/Omer/ProcessPaymentRequest", paymentGatewayUrl),
                    CancelUrl = string.Format("{0}/api/Omer/CancelPaymentRequest", paymentGatewayUrl),
                    Currency= client.CurrencyId,
                    BillingAddress = client.Address?.Trim(),
                    HolderName = string.Format("{0} {1}", client.FirstName, client.LastName),
                    PartnerDomain = session.Domain,
                    session.LanguageId,
                    ResourcesUrl = (resourcesUrlKey == null || resourcesUrlKey.Id == 0 ? ("https://resources." + session.Domain) : resourcesUrlKey.StringValue),
                    //ResourcesUrl = "https://gamingwebsitetest.craftbetstage.com/user/1/deposit",
                    CountryCode = paymentInfo.Country,
                    ZipCode = client.ZipCode?.Trim(),
                    paymentInfo.City,
                    Country = paymentInfo.Country,
                    client.PartnerId,
                    description = "Deposit",
                    region = region?.IsoCode ?? region?.IsoCode3,
                    ip = paymentInfo.TransactionIp,
                    Amount = amount,
                    PaymentSystemName = input.PaymentSystemName.ToLower()
                };

                var distributionUrl = string.Format(distributionUrlKey.StringValue,session.Domain);
                var data = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(paymentProcessingInput));
                return string.Format("{0}/paymentform/paymentprocessing?data={1}", distributionUrl, data);
            }
        }
    }
}
