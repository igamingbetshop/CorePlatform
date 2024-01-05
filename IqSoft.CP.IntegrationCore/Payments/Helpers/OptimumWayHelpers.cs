using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models.OptimumWay;
using System.Text;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Integration.Payments.Models;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class OptimumWayHelpers
    {
        public static string CallOptimumWayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                if (string.IsNullOrEmpty(client.Address)) // check if card
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
                if (string.IsNullOrEmpty(client.ZipCode?.Trim())) // check if card
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
                if (string.IsNullOrEmpty(client.FirstName?.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
                if (string.IsNullOrEmpty(client.LastName?.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);

                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OptimumWayApiUrl).StringValue;
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var amount = input.Amount;
                if (client.CurrencyId != Constants.Currencies.USADollar)
                {
                    var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.USADollar, partnerPaymentSetting);
                    amount = Math.Round(rate * input.Amount, 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.USADollar);
                    parameters.Add("AppliedRate", Math.Round(rate, 4).ToString());
                    parameters.Add("Amount", amount.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                var apikeys = partnerPaymentSetting.Password.Split(',');
                var apiKey = apikeys[0];
                var sharedSecret = apikeys[1];
                var paymentInput = JsonConvert.SerializeObject(new
                {
                    merchantTransactionId = input.Id.ToString(),
                    amount = amount.ToString("F"),
                    currency = Constants.Currencies.USADollar,
                    successUrl = cashierPageUrl,
                    cancelUrl = cashierPageUrl,
                    errorUrl = cashierPageUrl,
                    callbackUrl = string.Format("{0}/api/OptimumWay/ApiRequest", paymentGateway),
                    customer = new
                    {
                        firstName = client.FirstName,
                        lastName = client.LastName,
                        billingAddress1 = client.Address,
                        billingCity = paymentInfo.City,
                        billingPostcode = client.ZipCode.Trim(),
                        billingCountry = paymentInfo.Country,
                        billingState = paymentInfo.City
                    }
                });
                var byteArray = Encoding.Default.GetBytes(partnerPaymentSetting.UserName);
                var headers = new Dictionary<string, string>
                {
                    { "Authorization", "Basic " + Convert.ToBase64String(byteArray) },
                    { "X-Signature", CommonFunctions.ComputeHMACSha512(paymentInput, sharedSecret) }
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    RequestHeaders = headers,
                    Date = DateTime.Now.ToUniversalTime(),
                    Url = string.Format("{0}/transaction/{1}/debit", url, apiKey),
                    PostData = paymentInput
                };
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                input.ExternalTransactionId =  paymentOutput.PurchaseId;
                paymentSystemBl.ChangePaymentRequestDetails(input);

                if (!paymentOutput.Success)
                    throw new Exception(string.Format("ErrorCode: {0}, AdapterCode: {1}, AdapterMessage {2}", paymentOutput.Error[0].ErrorCode,
                                         paymentOutput.Error[0].AdapterCode, paymentOutput.Error[0].AdapterMessage));

                return paymentOutput.RedirectUrl;
            }
        }

        // should be updated, currently not available
        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log) 
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OptimumWayApiUrl).StringValue;
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var apikeys = partnerPaymentSetting.Password.Split(',');
                var apiKey = apikeys[0];
                var sharedSecret = apikeys[1];
                var payoutInput = JsonConvert.SerializeObject(new
                {
                    merchantTransactionId = paymentRequest.Id.ToString(),
                    amount = paymentRequest.Amount.ToString("F"),
                    currency = client.CurrencyId,
                    callbackUrl = string.Format("{0}/api/OptimumWay/ApiRequest", paymentGateway),
                    customer = new
                    {
                        firstName = client.FirstName,
                        lastName = client.LastName,
                        billingAddress1 = client.Address
                    }
                });
                var byteArray = Encoding.Default.GetBytes(partnerPaymentSetting.UserName);
                var headers = new Dictionary<string, string>
                {
                    { "Authorization", "Basic " + Convert.ToBase64String(byteArray) },
                    { "X-Signature", CommonFunctions.ComputeHMACSha512(payoutInput, sharedSecret) }
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    RequestHeaders = headers,
                    Date = DateTime.Now.ToUniversalTime(),
                    Url = string.Format("{0}/transaction/{1}/debit", url, apiKey),
                    PostData = payoutInput
                };
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                paymentRequest.ExternalTransactionId =  paymentOutput.PurchaseId;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);


                if (paymentOutput.Success)
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                    };
                throw new Exception(string.Format("ErrorCode: {0}, AdapterCode: {1}, AdapterMessage {2}", paymentOutput.Error[0].ErrorCode,
                                          paymentOutput.Error[0].AdapterCode, paymentOutput.Error[0].AdapterMessage));
            }
        }
    }
}