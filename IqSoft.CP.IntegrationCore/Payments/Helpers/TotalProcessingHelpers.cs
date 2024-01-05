using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.TotalProcessing;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class TotalProcessingHelpers
    {
        private class PaymentBrand
        {
            public readonly static string Visa = "VISA";
            public readonly static string Master = "MASTER";
        }

        private class PaymentType
        {
            public readonly static string Authorization = "PA";
            public readonly static string Debit = "DB";
            public readonly static string Credit = "CD";
        }
		
        private static Dictionary<string, string> PaymentWays { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.TotalProcessingVisa, "VISA" },
            { Constants.PaymentSystems.TotalProcessingMaster, "MASTER" },
            { Constants.PaymentSystems.TotalProcessingMaestro, "MAESTRO" },
            { Constants.PaymentSystems.TotalProcessingPaysafe, "PAYSAFECARD" }
        };


        public static string CallTotalProcessingApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var paymentSystem = CacheManager.GetPaymentSystemById(partnerPaymentSetting.PaymentSystemId);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.TotalProcessingUrl).StringValue;

                var paymentRequestInput = new PaymentRequestInput
                {
                    entityId = partnerPaymentSetting.UserName,
                    amount = string.Format("{0:N2}", input.Amount),
                    currency = input.CurrencyId,
                    paymentType = PaymentType.Debit,
                    merchantTransactionId = input.Id.ToString()
                };

                Dictionary<string, string> requestHeaders = new Dictionary<string, string> {
                { "Authorization", "Bearer " + partnerPaymentSetting.Password} };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    Url = string.Format("{0}/v1/checkouts", url),
                    RequestHeaders = requestHeaders,
                    PostData = CommonFunctions.GetUriDataFromObject(paymentRequestInput)+ string.Format("&customer.merchantCustomerId={0}", client.Id),
                };

				var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));

				if (string.IsNullOrEmpty(response.Id))
                    throw new Exception(response.Result.Description);
                input.ExternalTransactionId = response.Id;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                var distUrl = string.Format( CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl).StringValue, session.Domain);
                return string.Format("{0}/TotalProcessing/LoadPaymentPage?redirectUrl={1}&checkoutId={2}&paymentWay={3}&website={4}", 
					distUrl, Uri.EscapeDataString(cashierPageUrl), response.Id, PaymentWays[paymentSystem.Name], session.Domain);
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var documentBl = new DocumentBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(documentBl))
                        {
                            var client = CacheManager.GetClientById(input.ClientId);
                            if (client == null)
                                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);

                            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.TotalProcessingUrl).StringValue;

                            var returnUrl = string.Format("https://{0}", session.Domain);
                            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                            var paymentSystem = CacheManager.GetPaymentSystemById(partnerPaymentSetting.PaymentSystemId);
                            var amount = input.Amount - (input.CommissionAmount ?? 0);
                            var paymentRequestInput = new PaymentRequestInput
                            {
                                entityId = partnerPaymentSetting.UserName,
                                amount = string.Format("{0:N2}", amount),
                                currency = input.CurrencyId,
                                paymentBrand = PaymentWays[paymentSystem.Name],
                                paymentType = PaymentType.Credit,
                                merchantTransactionId = input.Id.ToString()//,
                                /*shopperResultUrl = string.Format("{0}/api/TotalProcessing/PayoutRequest", CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.PaymentGateway).StringValue)*/
                            };

                            Dictionary<string, string> requestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + partnerPaymentSetting.Password } };
                            var expDate = Convert.ToDateTime(paymentInfo.ExpirationDate);
                            var httpRequestInput = new HttpRequestInput
                            {
                                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                                RequestMethod = HttpMethod.Post,
                                Url = string.Format("{0}/v1/payments", url),
                                RequestHeaders = requestHeaders,
                                PostData = string.Format("{0}&card.number={1}&card.holder={2}&card.expiryMonth={3}&card.expiryYear={4}",
                                CommonFunctions.GetUriDataFromObject(paymentRequestInput), paymentInfo.CardNumber, paymentInfo.CardHolderName, expDate.Month.ToString("D2"), expDate.Year)
                            };

                            var output = CommonFunctions.SendHttpRequest(httpRequestInput, out _);

                            var response = JsonConvert.DeserializeObject<PaymentOutput>(output);

                            if (string.IsNullOrEmpty(response.Id))
                                throw new Exception(response.Result.Description);
                            if (response.Result.Code != "000.000.000")
                                throw new Exception(response.Result.Description);

                            input.ExternalTransactionId = response.Id;
                            paymentSystemBl.ChangePaymentRequestDetails(input);
                            httpRequestInput.RequestMethod = HttpMethod.Get;
                            httpRequestInput.PostData = string.Empty;
                            httpRequestInput.Url = string.Format("{0}/{1}?entityId={2}", httpRequestInput.Url, response.Id, partnerPaymentSetting.UserName);
                            response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));

                            if (string.IsNullOrEmpty(response.Id))
                                throw new Exception(response.Result.Description);
                            if (response.Result.Code != "000.000.000")
                                throw new Exception(response.Result.Description);

                            var resp = clientBl.ChangeWithdrawRequestState(input.Id, PaymentRequestStates.Approved,
                                string.Empty, null, null, true, input.Parameters, documentBl, notificationBl);
                            return new PaymentResponse
                            {
                                Status = PaymentRequestStates.Approved,
                            };
                        }
                    }
                }
            }
        }
    }
}