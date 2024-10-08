using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.PaymentGateway.Models.MaxPay;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class MaxPayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.FinVert);
        [HttpPost]
        [Route("api/MaxPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
            var userIds = new List<int>();
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + JsonConvert.SerializeObject(inputString));
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TransactionId));
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                                        paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
                            var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                            if (parameters.ContainsKey("PaymentSystem"))
                                parameters["PaymentSystem"] = input.PaymentSystem;
                            else
                                parameters.Add("PaymentSystem", input.PaymentSystem);
                           

                            //if (parameters.ContainsKey("InitialAmount"))
                            //    parameters["InitialAmount"] = paymentRequest.Amount.ToString("F");
                            //else
                            //    parameters.Add("InitialAmount", paymentRequest.Amount.ToString("F"));
                            //if (parameters.ContainsKey("UpdatedAmount"))
                            //    parameters["UpdatedAmount"] = input.Amount.ToString("F");
                            //else
                            //    parameters.Add("UpdatedAmount", input.Amount.ToString("F"));
                            //paymentRequest.Amount = input.Amount;
                            

                            paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            {
                                if (input.Status.ToLower() == "ok")
                                {
                                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds);
                                }
                                else if (input.Status.ToLower() == "error")
                                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted,
                                        string.Format("ErrorMessage: {0}", input.Message), notificationBl);
                            }
                            else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                            {
                                using (var documentBll = new DocumentBll(paymentSystemBl))
                                {
                                    if (input.Status.ToLower() == "ok")
                                    {
                                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                         null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    }
                                    else if (input.Status.ToUpper() == "error")
                                    {
                                        var reason = string.Format("ErrorMessage: {0}", input.Message );
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