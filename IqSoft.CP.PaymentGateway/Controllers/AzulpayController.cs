using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Azulpay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{

    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class AzulpayController : ApiController
    {

        [Route("api/Azulpay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = "OK";
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.CustomerOrderId));
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            if (input.TransactionStatus == "APPROVED")
                            {
                                clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                            }
                            else if (input.TransactionStatus == "DECLINED" || input.TransactionStatus == "ERROR")
                            {
                                clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, 
                                    $"Status: { input.TransactionStatus } Reason: { input.Reason }", notificationBl);
                            }
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null)
                {
                    if (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists || ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)
                        response = "OK";
                    else if (ex.Detail.Id == Constants.Errors.WrongHash)
                    {
                        response = ex.Detail.Id + " " + ex.Detail.NickName;
                    }
                }
                else
                {
                    response = ex.Message;
                }
                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(response);
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
        }
    }
}