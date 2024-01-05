using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using IqSoft.CP.Integration.Payments.Models.NOWPay;
using IqSoft.CP.PaymentGateway.Models.NOWPay;
using IqSoft.CP.Common.Models.CacheModels;
using System.IO;
using System.Web.Http;
using System.Web;
using System.Net;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.PaymentGateway.Helpers;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class NOWPayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.NOWPay);

        [HttpPost]
        [Route("api/NOWPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info(inputString);
                BaseBll.CheckIp(WhitelistedIps);

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Order_id));
                            if (request == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(request.ClientId.Value);

                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                               client.CurrencyId, (int)PaymentRequestTypes.Deposit);                           

                            request.ExternalTransactionId = input.Payment_id;
                            request.Info = JsonConvert.SerializeObject(input);
                            if (input.Payment_status.ToUpper() != "WAITING")
                                request.Amount = Math.Round(input.Price_amount * input.Actually_paid / input.Pay_amount, 2);
                            paymentSystemBl.ChangePaymentRequestDetails(request);
                            if (input.Payment_status.ToUpper() == "FINISHED" || input.Payment_status.ToUpper() == "PARTIALLY_PAID")
                            {
                                clientBl.ApproveDepositFromPaymentSystem(request, false);
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                            else if (input.Payment_status.ToUpper() == "FAILED" || input.Payment_status.ToUpper() == "EXPIRED")
                            {
                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.Order_description, notificationBl);
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && ex.Detail.Id != Constants.Errors.ClientDocumentAlreadyExists &&
                    ex.Detail.Id != Constants.Errors.RequestAlreadyPayed)
                {
                    input.Payment_status = "1";
                    input.Order_description = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                }
                WebApiApplication.DbLogger.Error(ex.Detail);
            }
            catch (Exception ex)
            {
                input.Payment_status = "-1";
                input.Order_description = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8)
            };
        }

        [HttpPost]
        [Route("api/NOWPay/PayoutRequest")]
        public HttpResponseMessage PayoutRequest(PayoutOutput input)
        {
            var response = "OK";
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info(inputString);
                BaseBll.CheckIp(WhitelistedIps);

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(clientBl))
                            {
                                var transaction = JsonConvert.DeserializeObject<Withdrawal>(inputString);
                                var clientId = 0;
                                if (transaction.Status.ToLower() == "finished")
                                {
                                    var request = paymentSystemBl.GetPaymentRequestByExternalId(transaction.Batch_withdrawal_id, CacheManager.GetPaymentSystemByName(PaymentSystems.NOWPay).Id);
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                                    null, null, false, string.Empty, documentBll, notificationBl);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    clientId = request.ClientId.Value;
                                }
                                else if (transaction.Status.ToLower() == "failed" || transaction.Status.ToLower() == "rejected")
                                {
                                    var request = paymentSystemBl.GetPaymentRequestByExternalId(transaction.Batch_withdrawal_id, CacheManager.GetPaymentSystemByName(PaymentSystems.NOWPay).Id);
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, string.Empty,
                                                    null, null, false, string.Empty, documentBll, notificationBl);
                                    clientId = request.ClientId.Value;
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(clientId);
                                BaseHelpers.BroadcastBalance(clientId);
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
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };

                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                WebApiApplication.DbLogger.Error(exp);
                response = exp.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = ex.Message;
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }
    }
}