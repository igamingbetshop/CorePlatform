using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Help2Pay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class Help2PayController : ApiController
	{
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Help2Pay);

        [HttpPost]
        [Route("api/Help2Pay/ApiRequest")]
        public HttpResponseMessage ApiRequest(RequestResultInput input)
        {
            var response = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var partnerBl = new PartnerBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            try
                            {
                                BaseBll.CheckIp(WhitelistedIps);
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Reference));
                                if (request == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                                var client = CacheManager.GetClientById(Convert.ToInt32(input.Customer));
                                if (client == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId, request.CurrencyId, (int)PaymentRequestTypes.Deposit);
                                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);

                                var signature = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                                                               input.Merchant, input.Reference,
                                                               input.Customer, input.Amount,
                                                               input.Currency, input.Status,
                                                               partnerPaymentSetting.Password);
                                signature = CommonFunctions.ComputeMd5(signature);
                                if (signature.ToLower() != input.Key.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);

                                if (input.Status == Help2PayHelpers.Statuses.Approved || input.Status == Help2PayHelpers.Statuses.Success)
                                {
                                    request.ExternalTransactionId = input.ID;
                                    paymentSystemBl.ChangePaymentRequestDetails(request);

                                    response = "State=OK";
                                    clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds);
                                    foreach (var uId in userIds)
                                    {
                                        PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                    }
                                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
                                }
                                if (input.Status == Help2PayHelpers.Statuses.Failed ||
                                    input.Status == Help2PayHelpers.Statuses.Rejected ||
                                    input.Status == Help2PayHelpers.Statuses.Canceled)
                                {
                                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.Status.ToString(), notificationBl);
                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("State=OK", Encoding.UTF8) };
                                }
                                else
                                {
                                    response = "State=RETRY";
                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(response, Encoding.UTF8) };
                                }
                            }
                            catch (FaultException<BllFnErrorType> ex)
                            {
                                if (ex.Detail != null &&
                                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                                {
                                    response = "State=OK";
                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
                                }
                                WebApiApplication.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
                                response = "State=RETRY&ErrorDescription=" + ex.Message;
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(response, Encoding.UTF8) };
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex);
                                response = "State=RETRY&ErrorDescription=" + ex.Message;
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(response, Encoding.UTF8) };
                            }
                        }
                    }
                }
            }
        }

        [HttpPost]
        [Route("api/Help2Pay/VerifyTransaction")]
        public HttpResponseMessage PayoutVerificationRequest([FromUri] CheckInput checkInput)
        {
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Help2Pay);
                    if (paymentSystem == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                    var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(checkInput.transId));
                    if (request == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(request.ClientId.Value);
                    if (client == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);

                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, request.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    if (partnerPaymentSetting == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(request.Info);
                    var signature = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                                                   partnerPaymentSetting.UserName, checkInput.transId,
                                                   client.Id, request.Amount.ToString("F"),
                                                   request.CurrencyId, request.LastUpdateTime.ToString("yyyyMMddHHmmss"),
                                                   paymentInfo.BankAccountNumber, partnerPaymentSetting.Password);
                    signature = CommonFunctions.ComputeMd5(signature);

                    //if (signature.ToLower() != checkInput.key.ToLower())
                    //    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);
                    return new HttpResponseMessage { Content = new StringContent("true", Encoding.UTF8) };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                WebApiApplication.DbLogger.Error(new Exception(ex.Detail.Message));
                return new HttpResponseMessage { Content = new StringContent("false", Encoding.UTF8) };
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return new HttpResponseMessage { Content = new StringContent("false", Encoding.UTF8) };
            }                          
        }

        [HttpPost]
        [Route("api/Help2Pay/PayoutResult")]
        public HttpResponseMessage PayoutRequest(PayoutResultInput input)
        {          
            var response = new ResultOutput
            {
                StatusCode = Help2PayHelpers.Statuses.Success,
                Message = "Succes"
            };
            var userIds = new List<int>();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBl = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Help2Pay);
                                if (paymentSystem == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TransactionID));
                                if (request == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                if (Convert.ToInt64(input.MemberCode) != client.Id)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);

                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, request.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                                if (partnerPaymentSetting == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(request.Info);
                                var signature = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                                                               input.MerchantCode, input.TransactionID,
                                                               input.MemberCode, Convert.ToDecimal(input.Amount).ToString("F"),
                                                               input.CurrencyCode, input.Status, partnerPaymentSetting.Password);
                                signature = CommonFunctions.ComputeMd5(signature);

                                if (signature.ToLower() != input.Key.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);

                                if (request.Amount != Convert.ToDecimal(input.Amount))
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestInValidAmount);
                                request.ExternalTransactionId = input.ID;
                                paymentSystemBl.ChangePaymentRequestDetails(request);
                                if (input.Status == Help2PayHelpers.Statuses.Success)//success
                                {
                                    var req = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                                                                  null, null, false, request.Parameters, documentBl, notificationBl, out userIds);
                                    clientBl.PayWithdrawFromPaymentSystem(req, documentBl, notificationBl);
                                }
                                else if (input.Status == Help2PayHelpers.Statuses.Failed && request.Status == (int)PaymentRequestStates.PayPanding)
                                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, input.Message, null, null, false,
                                                                        request.Parameters, documentBl, notificationBl, out userIds);

                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response.StatusCode = Help2PayHelpers.Statuses.Failed;
                response.Message = ex.Detail.Message;
                WebApiApplication.DbLogger.Error(new Exception(ex.Detail.Message));
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response.StatusCode = Help2PayHelpers.Statuses.Failed;
                response.Message = ex.Message;
            }
            var output =  CommonFunctions.ToXML(response);
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(output, Encoding.UTF8)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationXml);
            return resp;
        }
    }
}