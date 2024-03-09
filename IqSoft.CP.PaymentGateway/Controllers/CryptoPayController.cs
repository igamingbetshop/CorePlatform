using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.CryptoPay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [System.Web.Http.Cors.EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class CryptoPayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.CryptoPay);

        [HttpPost]
        [Route("api/CryptoPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage input)
        {
            var response = string.Empty;
            try
            {
                var inputString = input.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info("inputString:" + inputString);
                BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.Contains("X-Cryptopay-Signature"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var inputSignature = Request.Headers.GetValues("X-Cryptopay-Signature").FirstOrDefault();
                if (string.IsNullOrEmpty(inputSignature))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                WebApiApplication.DbLogger.Info("Signature:" + inputSignature);
                var paymentInput = JsonConvert.DeserializeObject<PaymentInput>(inputString);
                if (paymentInput.Details.Status == "completed" || paymentInput.Details.Status == "pending" ||
                    paymentInput.Details.Status.ToLower() == "cancelled" || paymentInput.Details.Status.ToLower() == "refunded")
                {
                    if (paymentInput.Type.ToLower() == "channelpayment")
                        PaymentRequest(paymentInput, inputString, inputSignature);
                    else if (paymentInput.Type.ToLower() == "coinwithdrawal")
                        PayoutRequest(paymentInput, inputString, inputSignature);
                }
                response = "OK";
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "OK";
                }
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        private void PayoutRequest(PaymentInput paymentInput, string inputString, string inputSignature)
        {
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt32(paymentInput.Details.CustomId));
                    if (paymentRequest == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);

                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                       client.CurrencyId, paymentRequest.Type);
                    var sign = CommonFunctions.ComputeHMACSha256(inputString, partnerPaymentSetting.Password.Split(',')[1]);
                    if (sign.ToLower() !=  inputSignature.ToLower())
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");
                    var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                    if (!parameters.ContainsKey("Callback_ExchangeDetails"))
                        parameters.Add("Callback_ExchangeDetails", JsonConvert.SerializeObject(paymentInput.Details.ExchangeDetails));
                    if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                    {
                        if (!parameters.ContainsKey("Fee"))
                            parameters.Add("Fee", paymentInput.Details.Fee.ToString());
                        else
                            parameters["Fee"] =  paymentInput.Details.Fee.ToString();
                        if (!parameters.ContainsKey("Fee Currency"))
                            parameters.Add("Fee Currency", paymentInput.Details.FeeCurrency.ToString());
                        else
                            parameters["Fee Currency"] = paymentInput.Details.FeeCurrency.ToString();
                        paymentInfo.PayAmount =Convert.ToDecimal(paymentInput.Details.PaidAmount);
                    }
                    else
                    {
                        if (!parameters.ContainsKey("Network Fee"))
                            parameters.Add("Network Fee", paymentInput.Details.NetworkFee.ToString());
                        else
                            parameters["Network Fee"] = paymentInput.Details.NetworkFee.ToString();
                        paymentInfo.PayAmount = Convert.ToDecimal(paymentInput.Details.ReceivedAmount);
                    }
                    paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentRequest.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                    {
                        using (var documentBl = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                if (paymentInput.Details.Status.ToLower() == "completed")
                                {
                                    var req = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, paymentInput.Details.Status,
                                                                                    null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
                                    clientBl.PayWithdrawFromPaymentSystem(req, documentBl, notificationBl);
                                }
                                else if (paymentInput.Details.Status.ToLower() == "cancelled" || paymentInput.Details.Status.ToLower() == "refunded")
                                    clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, paymentInput.Details.Status, null,
                                                                        null, false, paymentRequest.Parameters, documentBl, notificationBl);
                            }
                        }
                    }
                }
            }
        }

        private void PaymentRequest(PaymentInput paymentInput, string inputString, string inputSignature)
        {
            var client = CacheManager.GetClientById(Convert.ToInt32(paymentInput.Details.CustomId.Split('_')[0]));
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var currency = paymentInput.Details.PaidCurrency.ToUpper();
            if (currency == Constants.Currencies.USDT &&  paymentInput.Details.Network == "ethereum")
                currency = Constants.Currencies.USDTERC20;
            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.CryptoPay + currency);
            if (paymentSystem == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id,
                                                                               client.CurrencyId, (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
            if (partnerPaymentSetting.State != (int)PartnerPaymentSettingStates.Active)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingBlocked);
            var sign = CommonFunctions.ComputeHMACSha256(inputString, partnerPaymentSetting.Password.Split(',')[1]);
            if (sign.ToLower() !=  inputSignature.ToLower())
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
            if (client.State == (int)ClientStates.BlockedForDeposit || client.State == (int)ClientStates.FullBlocked ||
                client.State == (int)ClientStates.Suspended || client.State == (int)ClientStates.SuspendedWithWithdraw || 
                client.State == (int)ClientStates.Disabled)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientBlocked);
            if (paymentInput.Details.ReceivedAmount < 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
            var amount = paymentInput.Details.ReceivedAmount;
            var parameters = new Dictionary<string, string>();
            if (client.CurrencyId != paymentInput.Details.ReceivedCurrency)
            {
                var rate = BaseBll.GetCurrenciesDifference(paymentInput.Details.ReceivedCurrency, client.CurrencyId);
                parameters.Add("Currency", paymentInput.Details.ReceivedCurrency);
                parameters.Add("AppliedRate", rate.ToString("F"));
                amount = Math.Round(rate * paymentInput.Details.ReceivedAmount, 2);
            }
            var paymentRequest = new PaymentRequest
            {
                Type = (int)PaymentRequestTypes.Deposit,
                Amount = amount,
                ClientId = client.Id,
                CurrencyId = client.CurrencyId,
                PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                PartnerPaymentSettingId = partnerPaymentSetting.Id,
                ExternalTransactionId = paymentInput.Details.TxId
            };

            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var notificationBl = new NotificationBll(paymentSystemBl))
                    {
                        var regionPath = CacheManager.GetRegionPathById(client.RegionId);
                        var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
                        var cityPath = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City);
                        var city = string.Empty;
                        if (cityPath != null)
                            city = CacheManager.GetRegionById(cityPath.Id ?? 0, client.LanguageId)?.Name;
                        var paymentInfo = new PaymentInfo
                        {
                            Country = country?.IsoCode,
                            City = city,
                        };
                        paymentRequest.CountryCode = country?.IsoCode;
                        if (paymentInput.Details.ExchangeDetails != null && !parameters.ContainsKey("ExchangeDetails"))
                            parameters.Add("ExchangeDetails", JsonConvert.SerializeObject(paymentInput.Details.ExchangeDetails));
                        if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                        {
                            if (!parameters.ContainsKey("PaidAmount"))
                                parameters.Add("PaidAmount", paymentInput.Details.PaidAmount.ToString());
                            if (!parameters.ContainsKey("PaidCurrency"))
                                parameters.Add("PaidCurrency", paymentInput.Details.PaidCurrency.ToString());
                            paymentInfo.PayAmount = Convert.ToDecimal(paymentInput.Details.PaidAmount);
                        }
                        else
                        {
                            if (!parameters.ContainsKey("Network Fee"))
                                parameters.Add("Network Fee", paymentInput.Details.NetworkFee.ToString());
                            paymentInfo.PayAmount = Convert.ToDecimal(paymentInput.Details.ReceivedAmount);
                        }
                        paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                        paymentRequest.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        });
                        using (var scope = CommonFunctions.CreateTransactionScope())
                        {
                            var request = clientBl.CreateDepositFromPaymentSystem(paymentRequest, out LimitInfo info, false);
                            request.Amount = amount;
                            request.Parameters =  paymentRequest.Parameters;
                            paymentSystemBl.ChangePaymentRequestDetails(request);
                            PaymentHelpers.InvokeMessage("PaymentRequst", request.ClientId);
                            if (paymentInput.Details.Status.ToLower() == "completed")
                            {
                                if (request.Amount < partnerPaymentSetting.MinAmount || request.Amount > partnerPaymentSetting.MaxAmount)
                                {
                                    scope.Complete();
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestInValidAmount);
                                }
                                clientBl.ApproveDepositFromPaymentSystem(request, false, paymentInput.Details.Status);
                            }
                            else if (paymentInput.Details.Status.ToLower() == "cancelled" || paymentInput.Details.Status.ToLower() == "refunded")
                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, paymentInput.Details.Status, notificationBl);
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastDepositLimit(info);
                            scope.Complete();
                        }
                    }
                }
            }
        }
    }
}