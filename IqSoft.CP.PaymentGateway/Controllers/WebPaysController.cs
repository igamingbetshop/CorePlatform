using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.WebPays;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class WebPaysController : ApiController
    {
        [HttpPost]
        [Route("api/WebPays/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput paymentInput)
        {
            var response = "SUCCESS";
            var userIds = new List<int>();
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);
                WebApiApplication.DbLogger.Info("paymentInput: " + JsonConvert.SerializeObject(paymentInput));

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                using (var documentBl = new DocumentBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(paymentInput.reference)) ??
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                      client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                    if (paymentInput.bill_amt != paymentRequest.Amount)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestInValidAmount);
                    if (paymentInput.bill_currency != paymentRequest.CurrencyId)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);

                    paymentRequest.ExternalTransactionId =  paymentInput.transID;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                    if (paymentInput.order_status == 1)
                    {
                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds, paymentInput.response);
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                    }
                    else if (paymentInput.order_status == 2 || //Declined
                             paymentInput.order_status == 3 ||  //Refunded
                             paymentInput.order_status == 22 ||  //Expired
                             paymentInput.order_status == 23 ||  //Cancelled
                             paymentInput.order_status == 24) //Failed
                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, paymentInput.response, notificationBl);

                }
                foreach (var uId in userIds)
                {
                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
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