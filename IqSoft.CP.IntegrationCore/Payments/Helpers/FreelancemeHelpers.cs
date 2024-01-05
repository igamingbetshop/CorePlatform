using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Freelanceme;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class FreelancemeHelpers
    {
        public static string CallFreelancemeApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = string.Format("{0}/telegram/bot/pay", CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.FreelancemeUrl).StringValue);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var amount = input.Amount;
                if (input.CurrencyId != Constants.Currencies.UzbekistanSom)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.UzbekistanSom);
                    amount = rate * input.Amount;
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.UzbekistanSom );
                    parameters.Add("AppliedRate", rate.ToString("F") );
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }

                var paymentRequestInput = new
                {
                    auth = new
                    {
                        login = partnerPaymentSetting.UserName,
                        salt = input.Id,
                        hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}", partnerPaymentSetting.UserName, input.Id, partnerPaymentSetting.Password))
                    },
                    payment_amount = amount * 100,
                    external_transaction_id = input.Id,
                    callback_url = string.Format("{0}/{1}", paymentGateway, "api/Freelanceme/ApiRequest"),
                    success_url = cashierPageUrl,
                    fail_url = cashierPageUrl,
                    client = new { id = client.Id.ToString() }
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    Url = url,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var response = JsonConvert.DeserializeObject<BaseOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.ErrorDescription.Code != 0)
                    throw new Exception(response.ErrorDescription.Description);
                input.ExternalTransactionId = response.Response.ExternalId;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return response.Response.PayUrl;
            }
        }
        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = string.Format("{0}/recipient/pay", CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.FreelancemeUrl).StringValue);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                if (input.CurrencyId != Constants.Currencies.UzbekistanSom)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.UzbekistanSom);
                    amount *= rate;
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.UzbekistanSom);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }

                var paymentRequestInput = new
                {
                    auth = new
                    {
                        login = partnerPaymentSetting.UserName,
                        salt = input.Id,
                        hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}", partnerPaymentSetting.UserName, input.Id, partnerPaymentSetting.Password))
                    },
                    payment_currency = Constants.Currencies.UzbekistanSom,
                    payment_amount = amount * 100,
                    external_transaction_id = input.Id,
                    callback_url = string.Format("{0}/{1}", paymentGateway, "api/Freelanceme/PayoutRequest"),
                    recipient = new
                    {
                        id = client.Id,
                        card = new
                        {
                            card_number = paymentInfo.CardNumber
                        }
                    }
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    Url = url,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var response = JsonConvert.DeserializeObject<BaseOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.ErrorDescription.Code != 0)
                    throw new Exception(response.ErrorDescription.Description);
                input.ExternalTransactionId = response.Response.ExternalId;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                if (response.Response.Status == 1 || response.Response.Status == 2)
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                    };
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Failed,
                };
            }
        }

        public static void GetPayoutRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                using (var notificationBl = new NotificationBll(clientBl))
                {
                    var client = CacheManager.GetClientById(paymentRequest.ClientId);
                    if (client == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                        paymentRequest.PaymentSystemId, paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var url = string.Format("{0}/recipient/status", CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.FreelancemeUrl).StringValue);
                    var paymentRequestInput = new
                    {
                        auth = new
                        {
                            login = partnerPaymentSetting.UserName,
                            salt = string.Format("{0}_{0}", paymentRequest.Id),
                            hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}", partnerPaymentSetting.UserName,
                                                              string.Format("{0}_{0}", paymentRequest.Id), partnerPaymentSetting.Password))
                        },
                        payment_id = paymentRequest.ExternalTransactionId,
                        external_transaction_id = paymentRequest.Id
                    };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = System.Net.Http.HttpMethod.Post,
                        Url = url,
                        PostData = JsonConvert.SerializeObject(paymentRequestInput)
                    };
                    var response = JsonConvert.DeserializeObject<BaseOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    if (response.ErrorDescription.Code == 0)
                    {
                        using var documentBl = new DocumentBll(clientBl);
                        if (response.Response.Status == 1)
                        {
                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved,
                                string.Empty, null, null, false, string.Empty, documentBl, notificationBl);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                        }
                        else if (response.Response.Status != 2)
                        {
                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
                            string.Empty, null, null, false, string.Empty, documentBl, notificationBl);
                        }
                    }
                }
            }
        }
    }
}