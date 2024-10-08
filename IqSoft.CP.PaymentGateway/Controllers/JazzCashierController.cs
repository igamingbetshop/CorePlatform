using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.JazzCashier;
using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class JazzCashierController : ApiController
    {
        [HttpPost]
        [Route("api/JazzCashier/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
            var userIds = new List<int>();
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                //   BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Data.IdInternalTransaction));
                        if (request == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(request.ClientId.Value);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                  request.PaymentSystemId, client.CurrencyId, request.Type);

                   
                        if (input.Data.TransactionStatus.Id == 2 && input.Data.TransactionStatus.Name.ToUpper() == "APPROVED")
                        {
                            if (request.Type == (int)PaymentRequestTypes.Deposit)
                            {
                                clientBl.ApproveDepositFromPaymentSystem(request, false, out userIds);
                            }
                            else if (request.Type == (int)PaymentRequestTypes.Withdraw)
                            {
                                using (var documentBll = new DocumentBll(paymentSystemBl))
                                {
                                    using (var notificationBl = new NotificationBll(clientBl))
                                    {

                                        var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                         null, null, false, request.Parameters, documentBll, notificationBl, out userIds);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    }
                                }
                            }
                        }
                        else if (input.Data.TransactionStatus.Name.ToUpper() == "DECLINED" || input.Data.TransactionStatus.Name.ToUpper() == "CANCELLED" ||
                                input.Data.TransactionStatus.Name.ToUpper()== "REJECTED" || input.Data.TransactionStatus.Name.ToUpper() == "ERROR")
                        {
                            using (var documentBll = new DocumentBll(paymentSystemBl))
                            {
                                using (var notificationBl = new NotificationBll(paymentSystemBl))
                                {
                                    if (request.Type == (int)PaymentRequestTypes.Deposit)
                                        clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.Data.TransactionStatus.Name, notificationBl);
                                    else if (request.Type == (int)PaymentRequestTypes.Withdraw)
                                        clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, input.Data.TransactionStatus.Name, null,
                                                                            null, false, request.Parameters, documentBll, notificationBl, out userIds);
                                }
                            }
                        }
                        foreach (var uId in userIds)
                        {
                            PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                        }
                        PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                        BaseHelpers.BroadcastBalance(request.ClientId.Value);
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
    }
}