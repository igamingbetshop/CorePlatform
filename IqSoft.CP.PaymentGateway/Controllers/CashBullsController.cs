using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.PaymentGateway.Models.CashBulls;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using System.ServiceModel;
using System;
using System.Net;
using System.Text;
using IqSoft.CP.PaymentGateway.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.CacheModels;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class CashBullsController : ApiController
    {
        [HttpPost]
        [Route("api/CashBulls/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = "Failed";
            var userIds = new List<int>();
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                using (var documentBl = new DocumentBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId)) ??
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    if (paymentRequest.CurrencyId != input.Currency)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);
                    if (paymentRequest.Amount != input.Amount)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestInValidAmount);
                    paymentRequest.ExternalTransactionId =  input.PaymentId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                    if (input.StatusTransaction.ToLower() == "approved")
                    {
                        if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds, comment: input.Description);
                        else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                        {
                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, input.Description, null, null, false,
                                                                           paymentRequest.Parameters, documentBl, notificationBl, out userIds, changeFromPaymentSystem: true);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                        }
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                    }
                    else if (input.StatusTransaction.ToLower() == "declined")
                        if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Description, notificationBl);
                        else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Description, null, null,
                                                                false, paymentRequest.Parameters, documentBl, notificationBl, out userIds);

                    foreach (var uId in userIds)
                    {
                        PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                    }
                    response = "OK";
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
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Message);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }
    }
}