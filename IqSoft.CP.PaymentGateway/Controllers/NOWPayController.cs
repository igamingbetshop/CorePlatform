using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using IqSoft.CP.Integration.Payments.Models.NOWPay;
using IqSoft.CP.PaymentGateway.Models.NOWPay;
using IqSoft.CP.Common.Models.CacheModels;
using System.IO;
using System.Web.Http;
using System.Web;
using System.Net;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.PaymentGateway.Helpers;
using System.Net.Http.Headers;
using IqSoft.CP.DAL;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class NOWPayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.NOWPay);

        [HttpPost]
        [Route("api/NOWPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info(inputString);
                BaseBll.CheckIp(WhitelistedIps);

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId));
                            if (request == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(request.ClientId.Value);

                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                               client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                            request.ExternalTransactionId = input.PaymentId;
                            if (input.PaymentStatus.ToUpper() != "WAITING")
                                request.Amount = Math.Round(input.PriceAmount * input.Actually_paid / input.PayAmount, 2);
                            paymentSystemBl.ChangePaymentRequestDetails(request);
                            if (input.PaymentStatus.ToUpper() == "FINISHED" || input.PaymentStatus.ToUpper() == "PARTIALLY_PAID")
                            {
                                clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                            else if (input.PaymentStatus.ToUpper() == "FAILED" || input.PaymentStatus.ToUpper() == "EXPIRED")
                            {
                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.OrderDescription, notificationBl);
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && ex.Detail.Id != Constants.Errors.ClientDocumentAlreadyExists &&
                    ex.Detail.Id != Constants.Errors.RequestAlreadyPayed)
                {
                    input.PaymentStatus = "1";
                    input.OrderDescription = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                }
                WebApiApplication.DbLogger.Error(ex.Detail);
            }
            catch (Exception ex)
            {
                input.PaymentStatus = "-1";
                input.OrderDescription = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8)
            };
        }

        [HttpPost]
        [Route("api/NOWPay/PayoutRequest")]
        public HttpResponseMessage PayoutRequest(PayoutOutput input)
        {
            var response = "OK";
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info(inputString);
                BaseBll.CheckIp(WhitelistedIps);
                var userIds = new List<int>();

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(clientBl))
                            {
                                var transaction = JsonConvert.DeserializeObject<Withdrawal>(inputString);
                                var clientId = 0;
                                if (transaction.Status.ToLower() == "finished")
                                {
                                    var request = paymentSystemBl.GetPaymentRequestByExternalId(transaction.Batch_withdrawal_id, CacheManager.GetPaymentSystemByName(PaymentSystems.NOWPay).Id);
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                                    null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    clientId = request.ClientId.Value;
                                }
                                else if (transaction.Status.ToLower() == "failed" || transaction.Status.ToLower() == "rejected")
                                {
                                    var request = paymentSystemBl.GetPaymentRequestByExternalId(transaction.Batch_withdrawal_id, CacheManager.GetPaymentSystemByName(PaymentSystems.NOWPay).Id);
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, string.Empty,
                                                    null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                    clientId = request.ClientId.Value;
                                }
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(clientId);
                                BaseHelpers.BroadcastBalance(clientId);
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };

                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                WebApiApplication.DbLogger.Error(exp);
                response = exp.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = ex.Message;
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }

        [HttpPost]
        [Route("api/NowPay/CreatePayment")]
        public HttpResponseMessage CreatePayment(HttpRequestMessage httpRequestMessage)
        {
            var response = string.Empty;
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info("inputString:" + inputString);
                BaseBll.CheckIp(WhitelistedIps);
                var paymentInput = JsonConvert.DeserializeObject<PaymentInput>(inputString);
                if (!Int32.TryParse(paymentInput.OrderDescription, out int clientId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
                var client = CacheManager.GetClientById(clientId) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.NOWPay + paymentInput.PayCurrency.ToUpper()) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                var partnerConfig = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.AllowMultiCurrencyAccounts);
                var allowMultiCurrencyAccounts = partnerConfig != null && partnerConfig.Id > 0 && partnerConfig.NumericValue == 1;
                var paymentRequestCurrency = allowMultiCurrencyAccounts ? paymentInput.PriceCurrency : client.CurrencyId;
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id,
                                                                                   paymentRequestCurrency, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                if (partnerPaymentSetting.State != (int)PartnerPaymentSettingStates.Active)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingBlocked);
                if (client.State == (int)ClientStates.BlockedForDeposit || client.State == (int)ClientStates.FullBlocked ||
                    client.State == (int)ClientStates.Suspended || client.State == (int)ClientStates.Disabled)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientBlocked);
                var amount = Math.Round(paymentInput.PriceAmount * paymentInput.Actually_paid / paymentInput.PayAmount, 2);
                var paymentInfo = new PaymentInfo
                {
                    WalletNumber = paymentInput.PayAddress
                };
                var paymentRequest = new PaymentRequest
                {
                    Type = (int)PaymentRequestTypes.Deposit,
                    Amount = amount,
                    ClientId = client.Id,
                    CurrencyId = paymentRequestCurrency,
                    PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                    PartnerPaymentSettingId = partnerPaymentSetting.Id,
                    ExternalTransactionId = paymentInput.PaymentId,
                    Parameters = "{}",
                    Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    })
                };
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                using (var scope = CommonFunctions.CreateTransactionScope())
                {
                    var request = clientBl.CreateDepositFromPaymentSystem(paymentRequest, out LimitInfo info, false);
                    request.Amount = amount;
                    request.Parameters =  paymentRequest.Parameters;
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    PaymentHelpers.InvokeMessage("PaymentRequst", request.ClientId);
                    if (paymentInput.PaymentStatus.ToUpper() == "FINISHED" || paymentInput.PaymentStatus.ToUpper() == "PARTIALLY_PAID")
                    {
                        if (request.Amount < partnerPaymentSetting.MinAmount || request.Amount > partnerPaymentSetting.MaxAmount)
                        {
                            scope.Complete();
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestInValidAmount);
                        }
                        clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds, paymentInput.PaymentStatus);
                        foreach (var uId in userIds)
                        {
                            PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                        }
                    }
                    else if (paymentInput.PaymentStatus.ToUpper() == "FAILED" || paymentInput.PaymentStatus.ToUpper() == "EXPIRED")
                        clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, paymentInput.PaymentStatus, notificationBl);
                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                    BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                    BaseHelpers.BroadcastDepositLimit(info);
                    scope.Complete();
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
    }
}