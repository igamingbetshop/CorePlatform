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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IqSoft.CP.Integration.Payments.Models.MaxPay;
using IqSoft.CP.Integration.Payments.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class MaxPayHelpers
    {
        public static string CallMaxPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);               
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MaxPayApiUrl).StringValue;
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var amount = input.Amount;
                if (client.CurrencyId != Constants.Currencies.USADollar ||client.CurrencyId != Constants.Currencies.Euro || client.CurrencyId != Constants.Currencies.RussianRuble)
                {
                    var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.RussianRuble, partnerPaymentSetting);
                    amount = Math.Round(rate * input.Amount, 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.RussianRuble);
                    parameters.Add("AppliedRate", Math.Round(rate, 4).ToString());
                    parameters.Add("Amount", amount.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }

                var secretKey = partnerPaymentSetting.Password;               
                var paymentInput = JsonConvert.SerializeObject(new
                {
                    transaction_id = input.Id.ToString(),
                    amount = amount.ToString("F"),
                    currency = Constants.Currencies.USADollar,
                    payment_system = "CardGateTest",//"CardGate"
                    url = new  {
                        callback_url = string.Format("{0}/api/MaxPay/ApiRequest", paymentGateway),
                        fail_url = cashierPageUrl,
                        pending_url = cashierPageUrl,
                        success_url = cashierPageUrl
                    }
                });
                var byteArray = Encoding.Default.GetBytes(partnerPaymentSetting.UserName);
                var headers = new Dictionary<string, string>
                {
                    { "Auth", Convert.ToBase64String(byteArray) },
                    { "Sign", CommonFunctions.ComputeMd5(string.Format("{0}{1}", paymentInput, secretKey)) }
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = headers,                   
                    Url = string.Format("{0}/deposit/create", url),
                    PostData = paymentInput
                };
                log.Info(JsonConvert.SerializeObject(httpRequestInput));
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                log.Info(resp);

                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(resp);
                if (paymentOutput.Status.ToLower() == "created")
                {
                    input.ExternalTransactionId = paymentOutput.ExternalId;
                    paymentSystemBl.ChangePaymentRequestDetails(input);                    
                }
                else
                {
                    throw new Exception(string.Format("ErrorCode: {0}, ErrorMessage: {1}, Description: {2}", paymentOutput.Code,
                                         paymentOutput.Message, paymentOutput.Description));
                }

                return paymentOutput.Redirect.Url;
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.MaxPayApiUrl);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                var secretKey = partnerPaymentSetting.Password;
                if (client.CurrencyId != Constants.Currencies.USADollar || client.CurrencyId != Constants.Currencies.Euro || client.CurrencyId != Constants.Currencies.RussianRuble)
                {
                    var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.RussianRuble, partnerPaymentSetting);
                    amount = Math.Round(rate * input.Amount, 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.RussianRuble);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                

                var paymentRequestInput = new
                {                        
                    amount = (int)amount,
                    currency = client.CurrencyId,
                    payment_system = "TestCard",//Card
                    transaction_id = input.Id,
                    system_fields = new { },//?? Container for additional fields of payment system. You can specify it with your manager.
                    url = new 
                    {
                        callback_url = string.Format("{0}/api/MaxPay/ApiRequest", paymentGateway)                       
                    }

                };
                var byteArray = Encoding.Default.GetBytes(partnerPaymentSetting.UserName);
                var headers = new Dictionary<string, string>
                {
                    { "Auth", Convert.ToBase64String(byteArray) },
                    { "Sign", CommonFunctions.ComputeMd5(string.Format("{0}{1}", paymentRequestInput, secretKey)) }
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = headers,
                    Url = string.Format("{0}/deduce/create", url),
                    PostData = JsonConvert.SerializeObject(paymentRequestInput, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                };

                log.Info(JsonConvert.SerializeObject(httpRequestInput));

                var output = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var response = JsonConvert.DeserializeObject<PaymentOutput>(output);
                
                log.Info(output);
                log.Info(response);

                if (response.Status.ToLower() == "created")
                {
                    input.ExternalTransactionId = response.ExternalId;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                        Description = response.Message,
                    };
                }
              
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Failed,
                    Description = response.Message
                };
            }
        }

    }
}
