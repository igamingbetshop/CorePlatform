using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Integration.Payments.Models.Flexepin;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models.Cache;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class FlexepinHelpers
    {
        public static PaymentResponse RedeemVoucher(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            using (var clientBl = new ClientBll(paymentSystemBl))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.FlexepinApiUrl);
                var timestamp = (Int64)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                var methodName = "/status";
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Get,
                    RequestHeaders = GetSignature(HttpMethod.Get.ToString().ToUpper(), methodName, string.Empty, partnerPaymentSetting),
                    Url = $"{url}{methodName}",
                };
                var statusResult = JsonConvert.DeserializeObject<StatusOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (statusResult.Status.ToLower() != "alive")
                    throw new Exception($"Service status is {statusResult.Status}");
                methodName = $"/voucher/validate/{paymentInfo.Info}/{partner.Name}/{input.Id}";
                httpRequestInput.RequestHeaders = GetSignature(httpRequestInput.RequestMethod.ToString().ToUpper(), methodName, string.Empty, partnerPaymentSetting);
                httpRequestInput.Url = $"{url}{methodName}";
                var validateResult = JsonConvert.DeserializeObject<VoucherOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                input.ExternalTransactionId = validateResult.TransNo;
                if (validateResult.Result == "0" && validateResult.Status.ToUpper() == "ACTIVE")
                {
                    input.Amount = Math.Round(BaseBll.ConvertCurrency(validateResult.Currency, client.CurrencyId, validateResult.Value), 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                        JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("VoucherCurrency", validateResult.Currency);
                    parameters.Add("Amount", validateResult.Value.ToString("F"));
                    parameters.Add("AppliedRate", BaseBll.GetCurrenciesDifference(validateResult.Currency, client.CurrencyId).ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentInfo.VoucherNumber = validateResult.Serial;
                    input.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                else
                {
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    throw new Exception($"Code: {validateResult.Result}, Message: {validateResult.ResultDescription}");
                }
                methodName = $"/voucher/redeem/{paymentInfo.Info}/{partner.Name}/{input.Id}";
                var requestBody = JsonConvert.SerializeObject(new { customer_ip = session.LoginIp });
                httpRequestInput.RequestMethod = HttpMethod.Put;
                httpRequestInput.RequestHeaders = GetSignature(httpRequestInput.RequestMethod.ToString().ToUpper(), methodName, requestBody, partnerPaymentSetting);
                httpRequestInput.Url = $"{url}{methodName}";
                httpRequestInput.PostData = requestBody;
                var redeemResult = JsonConvert.DeserializeObject<VoucherOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (redeemResult.Result == "0" && redeemResult.Status.ToUpper() == "USED")
                {
                    input.ExternalTransactionId = redeemResult.TransNo;
                    paymentInfo.VoucherNumber = redeemResult.Serial;
                    input.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    clientBl.ApproveDepositFromPaymentSystem(input, false);
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Approved,
                        Description = redeemResult.ResultDescription
                    };
                }
                throw new Exception($"Code: {redeemResult.Result}, Message: {redeemResult.ResultDescription}");
            }
        }

        private static Dictionary<string, string> GetSignature(string method, string route, string body, BllPartnerPaymentSetting partnerPaymentSetting)
        {
            var timestamp = (Int64)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            var signature = CommonFunctions.ComputeHMACSha256($"{method}\n{route}\n{timestamp}\n{body}", partnerPaymentSetting.Password).ToLower();
            return new Dictionary<string, string> { { "AUTHENTICATION", $"HMAC {partnerPaymentSetting.UserName}:{signature}:{timestamp}" } };
        }
    }
}