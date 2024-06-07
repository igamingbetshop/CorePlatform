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
using IqSoft.CP.Integration.Payments.Models.FreedomPay;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class FreedomPayHelpers
    {
        private static readonly List<string> SupportedCurrenies = new List<string>
        { 
            Constants.Currencies.USADollar,
            Constants.Currencies.Euro,
            Constants.Currencies.BritainPound
        };
        public static string CallFreedomPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrWhiteSpace(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (string.IsNullOrWhiteSpace(client.Address))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
            if (string.IsNullOrWhiteSpace(client.ZipCode.Trim()))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
            if (string.IsNullOrEmpty(paymentInfo.City) || string.IsNullOrEmpty(paymentInfo.Country))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
            //if (!SupportedCurrenies.Contains(client.CurrencyId))
            //    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongCurrencyId); // add exchange
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               client.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.FreedomPayApiUrl).StringValue;
            var sign = $"{CommonFunctions.ComputeMd5(input.Id.ToString())}" +
                       $"{CommonFunctions.ComputeMd5(((int)input.Amount).ToString())}" +
                       $"{CommonFunctions.ComputeMd5(client.CurrencyId)}";
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var paymentRequestInput = new
            {
                ORDER_ID = input.Id.ToString(),
                AMOUNT = input.Amount,
                CURRENCY = client.CurrencyId,
                DESCRIPTION = partner.Name,
                SIGNATURE = CommonFunctions.ComputeSha512(sign),
                RETURN_URL = cashierPageUrl,
                CANCEL_URL = cashierPageUrl,
                FIRSTNAME = client.FirstName,
                LASTNAME = client.LastName,
                EMAIL = client.Email,
                ADDRESS = client.Address,
                ZIP = client.ZipCode.Trim(),
                CITY = paymentInfo.City,
                COUNTRY = paymentInfo.Country
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", partnerPaymentSetting.UserName } },
                Url = url,
                PostData = CommonFunctions.GetUriEndocingFromObject(paymentRequestInput)
            };
            var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (response.Code != "00")
                throw new Exception($"Code: {response.Code}, Description: {response.Message}");
            return response.RedirectUrl;
        }
    }
}
