using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using log4net;
using IqSoft.CP.Integration.Payments.Models.Spayz;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class SpayzHelpers
    {
        public static string CallSpayzApi(PaymentRequest input,  string cashierPageUrl, SessionIdentity sessionIdentity, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(sessionIdentity.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(sessionIdentity.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(sessionIdentity.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (string.IsNullOrEmpty(client.Address))
                throw BaseBll.CreateException(sessionIdentity.LanguageId, Constants.Errors.AddressCantBeEmpty);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.SpayzApiUrl);
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            var paymentInput = new
            {
                order = new
                {
                    moid = input.Id.ToString(),
                    amount = new
                    {
                        value = (int)(input.Amount*100), // check decimal point
                        currency = input.CurrencyId
                    }
                },
                endUser = new
                {
                    firstName = client.FirstName,
                    lastName = client.LastName,
                    email = client.Email,
                    ipAddress = sessionIdentity.LoginIp,
                    phone = client.MobileNumber
                },
                notifyUrl = $"{paymentGatewayUrl}/api/Spayz/ApiRequest",
                returnUrl = cashierPageUrl
            };
            var byteArray = Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}:{partnerPaymentSetting.Password}");
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(byteArray) } },
                Url = paymentSystem.Name == Constants.PaymentSystems.SpayzBankCard ? $"{url}/bankcard" : $"{url}/openbanking",
                PostData = JsonConvert.SerializeObject(paymentInput)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(response);
            if (!string.IsNullOrEmpty(paymentOutput?.Result?.Order?.Id))
            {
                using (var paymentSystemBl = new PaymentSystemBll(sessionIdentity, log))
                {
                    input.ExternalTransactionId =  paymentOutput?.Result?.Order?.Id;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
            }
            if (paymentOutput?.Status.ToLower() == "success")
            {
                var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                    distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);
                var distributionUrl = string.Format(distributionUrlKey.StringValue, sessionIdentity.Domain);
                var resourcesUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResourcesUrl).StringValue;
                if (string.IsNullOrEmpty(resourcesUrl))
                    resourcesUrl = $"https://resources.{sessionIdentity.Domain}";
                var formInput = new
                {
                    JsonString = response,
                    ResourcesUrl = resourcesUrl
                };
                var data = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(formInput));
                return string.Format("{0}/spayz/paymentprocessing?data={1}", distributionUrl, data);
            }
            throw new Exception($"Error: {response}");
        }
    }
}