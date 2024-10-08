using System;
using System.Collections.Generic;
using System.Net.Http;
using IqSoft.CP.PaymentGateway.Models.Jeton;
using System.Web.Http;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.ServiceModel;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class JetonController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Jeton);

        [HttpPost]
        [Route("api/Jeton/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
            var userIds = new List<int>();
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(input.OrderId);
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                                        paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
                            

                            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit && input.Type.ToUpper() == "PAY")
                            {
                                if (input.Status.ToUpper() == "SUCCESS")
                                {
                                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds);
                                }
                                else if (input.Status.ToUpper() == "ERROR") // to check
                                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Message, notificationBl);
                            }
                            else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw && input.Type.ToUpper() == "PAYOUT")
                            {
                                using (var documentBll = new DocumentBll(paymentSystemBl))
                                {
                                    if (input.Status.ToUpper() == "SUCCESS")
                                    {
                                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                         null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    }
                                    else if (input.Status.ToUpper() == "ERROR") // to check statuses
                                    {
                                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
                                            input.Message, null, null, false, string.Empty, documentBll, notificationBl, out userIds);
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
        [Route("api/Jeton/CashRequest")]
        public HttpResponseMessage CashRequest(CashInput input)
        {
            var response = string.Empty;
            var userIds = new List<int>();
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt32(input.ReferenceNo));
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                                        paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
                            var hash = CommonFunctions.ComputeMd5($"{partnerPaymentSetting.Password}.{paymentRequest.Id}." +
                                                                  $"{input.PaymentMethod}.{input.Amount}.{client.CurrencyId}");
                            if (paymentRequest.Amount != Convert.ToDecimal(input.Amount))
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
                            if (client.CurrencyId != input.Currency)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);
                            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit && input.PaymentMethod == "REDEEM_VOUCHER")
                            {
                                if (input.Status.ToUpper() == "APPROVED")
                                {
                                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds);
                                }
                                else if (input.Status.ToUpper() == "DECLINED")
                                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Message, notificationBl);
                            }
                            else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw /*&& input.PaymentMethod == "REDEEM_VOUCHER"*/)
                            {
                                using (var documentBll = new DocumentBll(paymentSystemBl))
                                {
                                    if (input.Status.ToUpper() == "APPROVED")
                                    {
                                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                         null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    }
                                    else if (input.Status.ToUpper() == "DECLINED")
                                    {
                                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
                                            input.Message, null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                    }
                                }
                            }
                            foreach (var uId in userIds)
                            {
                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                            }
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            response = JsonConvert.SerializeObject(new
                            {
                                token = CommonFunctions.ComputeMd5($"{partnerPaymentSetting.Password}." +
                                                                   $"{paymentRequest.Id}.{input.PaymentMethod}")
                            });
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
    }
}