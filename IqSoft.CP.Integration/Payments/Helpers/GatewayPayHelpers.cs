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
using IqSoft.CP.Integration.Payments.Models.GatewayPay;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class GatewayPayHelpers
    {
        public static string CallGatewayPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (string.IsNullOrEmpty(client.MobileNumber))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.Address))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
            if (string.IsNullOrWhiteSpace(client.ZipCode?.Trim()))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            if (string.IsNullOrEmpty(paymentInfo.City) || string.IsNullOrEmpty(paymentInfo.Country))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);

            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.GatewayPayApiUrl);
            var redirectUrl = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.GatewayPayRedirectUrl);
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;

            var paymentRequestInput = new
            {
                first_name = client.FirstName,
                last_name = client.LastName,
                address = client.Address,
                country = paymentInfo.Country,
                state = paymentInfo.City,
                city = paymentInfo.City,
                zip = client.ZipCode.Trim(),
                ip_address = session.LoginIp,
                email = client.Email,
                phone_no = client.MobileNumber,
                amount = Math.Round(input.Amount, 2),
                customer_order_id = input.Id.ToString(),
                currency = client.CurrencyId,
                response_url = cashierPageUrl,
                webhook_url = $"{paymentGatewayUrl}/api/GatewayPay/ApiRequest"
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.UserName } },
                Url = url,
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (paymentOutput.ResponseCode != 7)
                throw new Exception($"Code: {paymentOutput.ResponseCode}, Message: {paymentOutput.ResponseMessage}");
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                input.ExternalTransactionId = paymentOutput?.Data?.Transaction?.OrderId;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                if (!string.IsNullOrEmpty(paymentOutput.TreeDsUrl))
                    return paymentOutput.TreeDsUrl;
                return $"{redirectUrl}?responseCode={paymentOutput.ResponseCode}&responseMessage={paymentOutput.ResponseMessage}" +
                       $"&order_id={input.ExternalTransactionId}&customer_order_id={input.Id}";
            }
        }
    }
}