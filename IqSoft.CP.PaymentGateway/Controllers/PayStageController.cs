using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.PaymentGateway.Models.PayStage;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using System.ServiceModel;
using System;
using System.Net;
using System.Text;
using IqSoft.CP.PaymentGateway.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.CacheModels;
using System.IO;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class PayStageController : ApiController
    {
        [HttpPost]
        [Route("api/PayStage/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = "Failed";
            try
            {

                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream); // for log
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                using (var documentBl = new DocumentBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.ReferenceNo)) ??
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                       client.CurrencyId, paymentRequest.Type);

                    if (input.Details.CreditAmount != paymentRequest.Amount || 
                       (input.Status.ToLower() == "completed" && input.Details.DebitAmount != paymentRequest.Amount))
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
                    if (input.Status.ToLower() == "completed")
                    {
                        if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, comment: input.Status);
                        else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                        {
                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, input.Status,
                                                                           null, null, false, string.Empty, documentBl, notificationBl);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                        }
                    }
                    else if (input.Status.ToUpper() == "EXPIRED" || input.Status.ToUpper() == "FAILED")
                    {
                        if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Status, notificationBl);
                        else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Status,
                                                                null, null, false, string.Empty, documentBl, notificationBl);
                    }
                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                    BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
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