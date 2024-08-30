using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Nixxe;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class NixxeController : ApiController
    {
        [HttpPost]
        [Route("api/nixxe/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = "SUCCESS";
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                var inputSign = HttpContext.Current.Request.Headers.Get("x-signature");
                if (string.IsNullOrEmpty(inputSign))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);
                WebApiApplication.DbLogger.Info("inputSign: " + inputSign);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                using (var documentBl = new DocumentBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TrackingId)) ??
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                      client.CurrencyId, paymentRequest.Type);
                    var sign = CommonFunctions.ComputeSha1(inputString + partnerPaymentSetting.Password);
                    if (sign.ToUpper() != inputSign) 
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                    if (input.Amount != paymentRequest.Amount)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestInValidAmount);
                    if (input.Currency != paymentRequest.CurrencyId)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);

                    paymentRequest.ExternalTransactionId =  input.TransactionId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                    if (input.Status.ToLower() == "successful")
                    {
                        if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                        {
                            clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                        }
                        else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                        {
                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, input.Status,
                                                                           null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                        }

                    }
                    else if (input.Status.ToLower() == "expired" || input.Status.ToLower() == "rejected")
                    {
                        if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Status, notificationBl);
                        else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                        {
                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Status, null, null,
                                                               false, paymentRequest.Parameters, documentBl, notificationBl);
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
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