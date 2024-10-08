using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.OptimumWay;
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
    public class OptimumWayController : ApiController
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "??"
        };

        [HttpPost]
        [Route("api/OptimumWay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                var userIds = new List<int>();
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MerchantTransactionId));
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                                        paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
                            var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                            if (parameters.ContainsKey("PaymentMethod"))
                                parameters["PaymentMethod"] = input.PaymentMethod;
                            else
                               parameters.Add("PaymentMethod", input.PaymentMethod);

                            paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            {
                                if (input.Result.ToUpper() == "OK")
                                {
                                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds);
                                }
                                else if (input.Result.ToUpper() == "ERROR")
                                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted,
                                        string.Format("ErrorMessage: {0}, AdapterMessage {1}", input.Message, input.AdapterMessage), notificationBl);
                            }
                            else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                            {
                                using (var documentBll = new DocumentBll(paymentSystemBl))
                                {
                                    if (input.Result.ToUpper() == "OK")
                                    {
                                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                         null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    }
                                    else if (input.Result.ToUpper() == "ERROR")
                                    {
                                        var reason = string.Format("ErrorMessage: {0}, AdapterMessage {1}", input.Message, input.AdapterMessage);
                                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, 
                                            reason, null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                    }
                                }
                            }
                            foreach (var uId in userIds)
                            {
                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                            }
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
                {
                    response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                    httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                }
                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(response);
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }

            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
    }
}