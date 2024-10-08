using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using IqSoft.CP.PaymentGateway.Models.Mifinity;
using System.Web.Http;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class MifinityController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Mifinity);

        [HttpPost]
        [Route("api/Mifinity/PayRequest")]
        public HttpResponseMessage PayRequest(PaymentInput input)
        {
            var response = string.Empty;
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt32(input.TraceId));
                        if (paymentRequest == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                           client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        //if (paymentRequest.Amount != input.MoneyDetails.Amount)
                        //    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
                        //if (client.CurrencyId != input.MoneyDetails.Currency)
                        //    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);
                        var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.MifinityApiUrl);
                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = Constants.HttpRequestMethods.Get,
                            RequestHeaders = new Dictionary<string, string> { { "x-api-version", "1" }, { "key", partnerPaymentSetting.Password } },
                            Url = $"{url}/api/gateway/payment-status/{input.ValidationKey}"
                        };
                        var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                        var statusOutput = JsonConvert.DeserializeObject<StatusOutput>(resp);
                        if (statusOutput.PayloadData == null || !statusOutput.PayloadData.Any())
                            throw new Exception(resp);
                        paymentRequest.ExternalTransactionId = statusOutput.PayloadData[0].PaymentDetails.TransactionReference;
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                        if (statusOutput.PayloadData[0].Status.ToUpper() == "SUCCESSFUL")
                        {
                            clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds);
                            foreach (var uId in userIds)
                            {
                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                            }
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                        }
                        else if (statusOutput.PayloadData[0].Status.ToUpper() == "FAILED")
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, statusOutput.PayloadData[0].ErrorMessage, notificationBl);
                            }
                        response = "OK";
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                   (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "OK";
                }
                else
                    response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error($"Error: {ex.Message}, Input: {JsonConvert.SerializeObject(input)}");
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error($"Error: {ex.Message}, Input: {JsonConvert.SerializeObject(input)}");
            }

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("api/Mifinity/RejectRequest")]
        public HttpResponseMessage RejectRequest(PaymentInput input)
        {
            var response = string.Empty;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt32(input.TraceId));
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.ErrorMessage, notificationBl);

                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            response = "OK";
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                   (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "OK";
                }
                else
                    response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(response);
            }

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("api/Mifinity/PayoutRequest")]
        public HttpResponseMessage PayoutRequest(PayoutInput input)
        {
            var response = string.Empty;
            var userIds = new List<int>();
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);               

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt32(input.TraceId));
                                if (paymentRequest == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                                if (input.TransactionStatus == 3) //SUBMITTED
                                {
                                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                    var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                                                                   null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                }
                                else if (input.TransactionStatus == 6)
                                    clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
                                           input.TransactionStatusDescription, null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
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
                {
                    response = "OK";
                }
                else
                    response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error($"Error: {ex.Message}, Input: {JsonConvert.SerializeObject(input)}");
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error($"Error: {ex.Message}, Input: {JsonConvert.SerializeObject(input)}");
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