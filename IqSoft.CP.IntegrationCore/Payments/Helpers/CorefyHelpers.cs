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
using IqSoft.CP.Common.Helpers;
using System.Collections.Generic;
using System.Text;
using IqSoft.CP.Integration.Payments.Models.Corefy;
using IqSoft.CP.Integration.Payments.Models;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class CorefyHelpers
    {
        private static Dictionary<string, string> PaymentServices { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.CorefyCreditCard, "payment_card_{0}_hpp"}, //payment_card_usd_hpp
            { Constants.PaymentSystems.CorefyBankTransfer,"bank_transfer_{0}_hpp"},
            { Constants.PaymentSystems.CorefyHavale, "" },
            { Constants.PaymentSystems.CorefyPep, "pep_{0}_hpp"},
            { Constants.PaymentSystems.CorefyPayFix, "payfix_{0}_hpp" },
            { Constants.PaymentSystems.CorefyMefete, "mefete_{0}_hpp"},
            { Constants.PaymentSystems.CorefyParazula, "parazula_{0}_hpp"},
            { Constants.PaymentSystems.CorefyPapara, "papara_{0}_hpp" }
        };

        private static Dictionary<string, string> PayoutServices { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.CorefyCreditCard, "payment_card_{0}"}, //payment_card_usd_hpp
            { Constants.PaymentSystems.CorefyBankTransfer,"bank_transfer_{0}"},
            { Constants.PaymentSystems.CorefyHavale, "" },
            { Constants.PaymentSystems.CorefyPep, "pep_{0}"},
            { Constants.PaymentSystems.CorefyPayFix, "payfix_{0}" },
            { Constants.PaymentSystems.CorefyMefete, "mefete_{0}"},
            { Constants.PaymentSystems.CorefyParazula, "parazula_{0}"},
            { Constants.PaymentSystems.CorefyPapara, "papara_{0}" }
        };

        public static string CallCorefyApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.CorefyApiUrl);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                if (!PaymentServices.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);
                var paymentInput = new
                {
                    data = new
                    {
                        type = "payment-invoices",
                        attributes = new
                        {
                            service = string.Format(PaymentServices[paymentSystem.Name], client.CurrencyId.ToLower()),
                            reference_id = input.Id.ToString(),
                            currency = client.CurrencyId,
                            amount = Math.Round(input.Amount, 2),
                            customer = new
                            {
                                reference_id = client.Id.ToString(),
                            },
                            test_mode = true, // change
                            return_url = cashierPageUrl,
                            callback_url = $"{paymentGateway}/api/Corefy/ApiRequest"
                        }
                    }
                };
                var basicAuth = Convert.ToBase64String(Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}"));
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Basic {basicAuth}" } },
                    Url = $"{url}/payment-invoices",
                    PostData = JsonConvert.SerializeObject(paymentInput)
                };
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(resp);
                if (paymentOutput.Data != null && paymentOutput.Data.Attributes != null)
                {
                    input.ExternalTransactionId =  paymentOutput.Data.Id;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    if (paymentOutput.Data.Attributes.Resolution == "ok")
                    {
                        var flowData = JsonConvert.DeserializeObject<FlowData>(JsonConvert.SerializeObject(paymentOutput.Data.Attributes.FlowData));
                        return flowData.Action;
                    }
                    throw new Exception(paymentOutput.Data.Attributes.Resolution);
                }
                throw new Exception(resp);
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.CorefyApiUrl);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                if (!PayoutServices.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var paymentInput = new
                {
                    data = new
                    {
                        type = "payout-invoice",
                        attributes = new
                        {
                            service = string.Format(PaymentServices[paymentSystem.Name], client.CurrencyId.ToLower()),
                            reference_id = paymentRequest.Id.ToString(),
                            currency = client.CurrencyId,
                            amount,
                            fields = new
                            {
                                card_number = paymentInfo.CardNumber
                            },
                            customer = new
                            {
                                reference_id = client.Id.ToString(),
                            },
                            test_mode = true, // change
                            callback_url = $"{paymentGateway}/api/Corefy/ApiRequest"
                        }
                    }
                };
                var basicAuth = Convert.ToBase64String(Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}"));
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Basic {basicAuth}" } },
                    Url = $"{url}/payout-invoices",
                    PostData = JsonConvert.SerializeObject(paymentInput)
                };
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(resp);
                if (paymentOutput.Data != null && paymentOutput.Data.Attributes != null)
                {
                    paymentRequest.ExternalTransactionId =  paymentOutput.Data.Id;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    if (paymentOutput.Data.Attributes.Resolution == "ok")
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding,
                        };
                    throw new Exception(paymentOutput.Data.Attributes.Resolution);
                }
                throw new Exception(resp);
            }
        }

        //public static void GetPaymentMethods(int partnerId)
        //{
        //    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Corefy);
        //    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, paymentSystem.Id,
        //                                                                       "EUR", (int)PaymentRequestTypes.Deposit);
        //    var url = CacheManager.GetPartnerPaymentSystemByKey(partnerId, paymentSystem.Id, Constants.PartnerKeys.CorefyApiUrl);
        //    var basicAuth = Convert.ToBase64String(Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}:{partnerPaymentSetting.Password}"));
        //    var httpRequestInput = new HttpRequestInput
        //    {
        //        ContentType = Constants.HttpContentTypes.ApplicationJson,
        //        RequestMethod = Constants.HttpRequestMethods.Get,
        //        RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Basic {basicAuth}" } },
        //        Url = $"{url}/payment-services"
        //    };
        //    var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        //}
    }
}