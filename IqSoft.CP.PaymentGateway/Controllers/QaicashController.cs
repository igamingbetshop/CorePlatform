using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.PaymentGateway.Models.Qaicash;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using System.ServiceModel;
using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class QaicashController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Qaicash);
        [HttpPost]
        [Route("api/Qaicash/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = "Failed";
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                // BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var request = paymentSystemBl.GetPaymentRequestById(input.OrderId);
                            if (request == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(request.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                        request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            var messageAuthenticationCode = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", input.OrderId, input.TransactionId, input.DateCreated,
                             input.DepositMethod, input.Amount, client.CurrencyId, input.Status, input.DateUpdated, input.DepositorUserId).Replace("||", "|");
                            messageAuthenticationCode = CommonFunctions.ComputeHMACSha256(messageAuthenticationCode, partnerPaymentSetting.Password).ToLower();

                            if (messageAuthenticationCode != input.MessageAuthenticationCode.ToLower())
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            var amount = Convert.ToDecimal(input.Amount);
                            if (amount != request.Amount)
                            {
                                var parameters = string.IsNullOrEmpty(request.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
                                parameters.Add("BeforeAmount", request.Amount.ToString());
                                parameters.Add("AdjustmentAmount", amount.ToString() );
                                request.Parameters = JsonConvert.SerializeObject(parameters);
                                request.Amount = amount;
                                paymentSystemBl.ChangePaymentRequestDetails(request);
                            }

                            if (input.Status.ToUpper() == "SUCCESS")
                            {
                                clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds, comment: input.Status);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                            else if (input.Status.ToUpper() == "FAILED")
                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.Status, notificationBl);
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
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(response);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }

        [HttpPost]
        [Route("api/Qaicash/PayoutRequest")]
        public HttpResponseMessage PayPaymentRequest(PayoutInput input)
        {
            string response;
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var userIds = new List<int>();
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var paymentSystemBl = new PaymentSystemBll(clientBl))
                    {
                        using (var documentBl = new DocumentBll(clientBl))
                        {
                            using (var notificationBl = new NotificationBll(clientBl))
                            {
                                var request = paymentSystemBl.GetPaymentRequestById(input.OrderId);
                                if (request == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                                var messageAuthenticationCode = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}", input.OrderId, input.TransactionId,
                                                                input.DateCreated, input.Amount, client.CurrencyId, input.Status,
                                                                input.DateUpdated, input.UserId).Replace("||", "|");
                                messageAuthenticationCode = CommonFunctions.ComputeHMACSha256(messageAuthenticationCode, partnerPaymentSetting.Password).ToLower();

                                if (messageAuthenticationCode != input.MessageAuthenticationCode.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                                request.ExternalTransactionId = input.TransactionId;
                                paymentSystemBl.ChangePaymentRequestDetails(request);
                                if (input.Status.ToUpper() == "SUCCESS")
                                {
                                    var req = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, input.InstrumentId.ToString(),
                                                                                  null, null, false, string.Empty, documentBl, notificationBl, out userIds);
                                    clientBl.PayWithdrawFromPaymentSystem(req, documentBl, notificationBl);
                                }
                                else if (input.Status.ToUpper() == "FAILED")
                                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, input.Notes, 
                                                                        null, null, false,string.Empty, documentBl, notificationBl, out userIds);

                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };
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
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
                response = exp.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex);
                response = ex.Message;
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }
    }
}
