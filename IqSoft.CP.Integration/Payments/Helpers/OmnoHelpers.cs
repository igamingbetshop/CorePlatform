using IqSoft.CP.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Integration.Payments.Models.Omno;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class OmnoHelpers
    {
        public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OmnohAuthenticationUrl).StringValue;
            var data = new
            {
                grant_type = "client_credentials",
                client_id = partnerPaymentSetting.UserName,
                client_secret = partnerPaymentSetting.Password
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                PostData = CommonFunctions.GetUriDataFromObject(data)
            };

            var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var auth = JsonConvert.DeserializeObject<AuthOutput>(res);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
            url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OmnoUrl).StringValue;
            var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var errorPageUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentErrorPageUrl).StringValue;
            var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                              JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);



            #region Check
            if (string.IsNullOrEmpty(client.FirstName?.Trim()))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName?.Trim()))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.Address))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
            if (string.IsNullOrEmpty(client.ZipCode?.Trim()))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
            if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (string.IsNullOrEmpty(client.MobileNumber))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
            if (string.IsNullOrEmpty(client.CurrencyId))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId);
            if (client.RegionId == 0)
                client.RegionId = Constants.DefaultRegionId;
            #endregion


            var amount = input.Amount;
            if (input.CurrencyId != Constants.Currencies.USADollar)
            {
                var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.USADollar);
                amount = Math.Round(rate * input.Amount, 2);
                parameters.Add("Currency", client.CurrencyId);
                parameters.Add("AppliedRate", rate.ToString("F"));
            }

            input.Parameters = JsonConvert.SerializeObject(parameters);


            using (var paymentSystemBl = new PaymentSystemBll(session, log)) 
            {
                paymentSystemBl.ChangePaymentRequestDetails(input);
            }
           
            var paymentInput = new
            {
                merchantTransactionId = input.Id.ToString(),
                amount = amount,
                currency = Constants.Currencies.USADollar,
                hookUrl = string.Format("{0}/api/Omno/ApiRequest", paymentGateway),
                callback = cashierPageUrl,
                callbackFail = errorPageUrl,
                billing = new
                {
                    firstName = client.FirstName,
                    lastName = client.LastName,
                    address1 = client.Address?.Trim(),
                    city = paymentInfo.City,
                    state = paymentInfo.City,
                    country = paymentInfo.Country,
                    postalCode = client.ZipCode.Trim(),
                    phone = client.MobileNumber,
                    email = client.Email,
                    externalUserId = client.Id.ToString(),
                },
                lang = session.LanguageId,
                orderId = input.Id.ToString(),
            };

            var headers = new Dictionary<string, string> { { "Authorization", $"Bearer {auth.AccessToken}" } };
            
            httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = headers,
                Url = url,
                PostData = JsonConvert.SerializeObject(paymentInput)
            };
            log.Info("OmnoHelpers httpRequestInput" + JsonConvert.SerializeObject(httpRequestInput));
            res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(res);

            if (paymentOutput.PaymentId != null)
            {
                using (var paymentSystemBl = new PaymentSystemBll(session, log))
                {
                    input.ExternalTransactionId = paymentOutput.PaymentId;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
            }
            log.Info("OmnoHelpers Output"+JsonConvert.SerializeObject(paymentOutput));
            return paymentOutput.PaymentUrlIFrame;
        }

    }
}
