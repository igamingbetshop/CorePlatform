using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.FinalPay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class FinalPayHelpers
    {
        public static string CallFinalPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.Password))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.FinalPayApiUrl).StringValue;
                var currentTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sss");

                var paymentRequestInput = new PaymentInput
                {
                    DataDetails = new RequestData
                    {
                        RequestType = "pay-in",
                        ReturnUrl = cashierPageUrl,
                        NotificationUrl = string.Format("{0}/api/FinalPay/ApiRequest", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue),
                        Language = CommonHelpers.LanguageISO5646Codes.ContainsKey(session.LanguageId) ? CommonHelpers.LanguageISO5646Codes[session.LanguageId] : "en-Us",
                        Timestamp = currentTime,
                        PayDetails = new Pay
                        {
                            OrderId = input.Id.ToString(),
                            Amount = input.Amount.ToString("0.##"),
                            Currency = input.CurrencyId
                        },
                        Customer = new Customer
                        {
                            ClientId = client.Id.ToString(),
                        },
                    }
                };
                paymentRequestInput.DataDetails.CheckSum = CommonFunctions.ComputeSha256(ConcatParams(paymentRequestInput) + partnerPaymentSetting.Password);
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    Url = url,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var resp = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (resp.State.ToUpper() == "INITIATED")
                {
                    input.ExternalTransactionId = resp.Data.Trans_ref;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    return resp.Data.Redirect_url;
                }
                throw new Exception(resp.Msg);

            }
        }
        public static string ConcatParams(object source)
        {
            var paramsList = new List<string>();
            var properties = source.GetType().GetProperties();
            foreach (var field in properties)
            {
                var value = field.GetValue(source, null);
                if (value == null)
                    continue;
                if (field.PropertyType.IsValueType || Type.GetTypeCode(field.PropertyType) == TypeCode.String)
                    paramsList.Add(value.ToString());
                else
                    paramsList.Add(ConcatParams(value));
            }
            return string.Join(string.Empty, paramsList);
        }

        public static string CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using var paymentSystemBl = new PaymentSystemBll(session, log);
            using var regionBl = new RegionBll(paymentSystemBl);
            var client = CacheManager.GetClientById(paymentRequest.ClientId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.FinalPayApiUrl).StringValue;
            var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);

            var currentTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sss");
            var redirectUrl = string.Format("https://{0}/user/1/withdraw/", session.Domain);
            var regionPath = regionBl.GetRegionPath(client.RegionId);
            var fnCountry = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
            var fnCity = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City);
            var cityName = string.Empty;
            var countryName = string.Empty;
            if (fnCity != null)
                cityName = CacheManager.GetRegionById(fnCity.Id.Value, Constants.DefaultLanguageId).Name;
            if (fnCountry != null)
                countryName = CacheManager.GetRegionById(fnCountry.Id.Value, Constants.DefaultLanguageId).Name;
            var payoutRequestInput = new PaymentInput
            {
                DataDetails = new RequestData
                {
                    RequestType = "pay-out",
                    ReturnUrl = redirectUrl,
                    NotificationUrl = string.Format("{0}/api/FinalPay/ApiRequest", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue),
                    Language = CommonHelpers.LanguageISO5646Codes.ContainsKey(session.LanguageId) ? CommonHelpers.LanguageISO5646Codes[session.LanguageId] : "en-Us",
                    Timestamp = currentTime,
                    PayDetails = new Pay
                    {
                        RequestType = "pay-out",
                        OrderId = paymentRequest.Id.ToString(),
                        Amount = amount.ToString("0.##"),
                        Currency = paymentRequest.CurrencyId
                    },
                    Customer = new Customer
                    {
                        ClientId = client.Id.ToString(),
                        FirstName = client.FirstName,
                        LastName = client.LastName,
                        Email = client.Email,
                        Address = client.Address,
                        City =  cityName,
                        Country = countryName,
                        Zip = "123",// dummy
                        Phone = client.MobileNumber,
                        BirthDate = client.BirthDate.ToString(),
                        RequestorIp = session.LoginIp

                    }
                }
            };
            payoutRequestInput.DataDetails.CheckSum = CommonFunctions.ComputeSha256(ConcatParams(payoutRequestInput) + partnerPaymentSetting.Password);

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = HttpMethod.Post,
                Url = url,
                PostData = JsonConvert.SerializeObject(payoutRequestInput)
            };
            var payoutRequestOutput = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (payoutRequestOutput.State.ToUpper() == "INITIATED")
            {
                paymentRequest.ExternalTransactionId = payoutRequestOutput.Data.Trans_ref;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                return payoutRequestOutput.Data.Redirect_url;
            }
            throw new Exception(string.Format("Code: {0}, Error: {1}", payoutRequestOutput.State, payoutRequestOutput.Msg));
        }
    }
}