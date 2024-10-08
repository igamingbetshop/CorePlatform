using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Models.EasyPay;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using System.IO;
using System.Xml.Serialization;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class EasyPayController : ApiController
    {

        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.EasyPay);
        [HttpPost]
        [Route("api/EasyPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage input)
        {
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            BaseBll.CheckIp(WhitelistedIps);
                            var inputString = input.Content.ReadAsStringAsync();
                            var serializer = new XmlSerializer(typeof(payfrexresponse), new XmlRootAttribute("payfrex-response"));
                            var inputObject = (payfrexresponse)serializer.Deserialize(new StringReader(inputString.Result));
                            WebApiApplication.DbLogger.Info(inputString.Result);

                            var request = paymentSystemBl.GetPaymentRequestById(inputObject.operations.operation.merchantTransactionId);
                            if (request == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(request.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            request.ExternalTransactionId = inputObject.operations.operation.payFrexTransactionId.ToString();
                            paymentSystemBl.ChangePaymentRequestDetails(request);
                            if ((!string.IsNullOrEmpty(inputObject.status) && inputObject.status.ToUpper() == "SUCCESS") &&
                                (!string.IsNullOrEmpty(inputObject.operations.operation.status) && inputObject.operations.operation.status.ToUpper() == "SUCCESS")
                                )
                            {
                                var pInfo = PaymentHelpers.RegisterClientPaymentAccountDetails(new DAL.ClientPaymentInfo
                                {
                                    Type = (int)ClientPaymentInfoTypes.CreditCard,
                                    CardNumber = inputObject.operations.operation.paymentDetails.cardNumber,
                                    ClientFullName = inputObject.operations.operation.paymentDetails.cardHolderName,
                                    WalletNumber = inputObject.operations.operation.paymentDetails.cardNumberToken,
                                    PartnerPaymentSystemId = request.PartnerPaymentSettingId,
                                    CreationTime = request.CreationTime,
                                    LastUpdateTime = request.LastUpdateTime,
                                    ClientId = request.ClientId.Value,
                                    AccountNickName = Constants.PaymentSystems.EasyPayCard
                                });
                                clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds, string.Empty, pInfo);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                            else if ((!string.IsNullOrEmpty(inputObject.status) && (inputObject.status.ToUpper() == "FAIL" || inputObject.status.ToUpper() == "ERROR")) ||
                                     (!string.IsNullOrEmpty(inputObject.operations.operation.status) &&
                                     (inputObject.operations.operation.status.ToUpper() == "FAIL" || inputObject.operations.operation.status.ToUpper() == "ERROR")))
                            {
                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, inputObject.operations.operation.message, notificationBl);
                            }
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };
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
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };
                }
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

                WebApiApplication.DbLogger.Error(exp);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(exp.Message, Encoding.UTF8) };
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(ex.Message, Encoding.UTF8) };
            }
        }
            
        [HttpPost]
        [Route("api/EasyPay/PayoutRequest")]
        public HttpResponseMessage PayoutRequest(HttpRequestMessage input)
        {
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var documentBl = new DocumentBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            try
                            {
                                BaseBll.CheckIp(WhitelistedIps);
                                var userIds = new List<int>();
                                var inputString = input.Content.ReadAsStringAsync();
                                var serializer = new XmlSerializer(typeof(payfrexresponse), new XmlRootAttribute("payfrex-response"));
                                var inputObjcet = (payfrexresponse)serializer.Deserialize(new StringReader(inputString.Result));
                                WebApiApplication.DbLogger.Info(inputString.Result);

                                var request = paymentSystemBl.GetPaymentRequestById(inputObjcet.operations.operation.merchantTransactionId);
                                if (request == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                                request.ExternalTransactionId = inputObjcet.operations.operation.payFrexTransactionId.ToString();
                                paymentSystemBl.ChangePaymentRequestDetails(request);
                                if ((!string.IsNullOrEmpty(inputObjcet.status) && inputObjcet.status.ToUpper() == "SUCCESS") &&
                                    (!string.IsNullOrEmpty(inputObjcet.operations.operation.status) && inputObjcet.operations.operation.status.ToUpper() == "SUCCESS"))
                                {
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                               null, null, false, string.Empty, documentBl, notificationBl, out userIds);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                }
                                else if ((!string.IsNullOrEmpty(inputObjcet.status) && (inputObjcet.status.ToUpper() == "FAIL" || inputObjcet.status.ToUpper() == "ERROR")) ||
                                         (!string.IsNullOrEmpty(inputObjcet.operations.operation.status) &&
                                         (inputObjcet.operations.operation.status.ToUpper() == "FAIL" || inputObjcet.operations.operation.status.ToUpper() == "ERROR")))
                                {
                                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, 
                                        inputObjcet.operations.operation.message, null, null, false, string.Empty, documentBl, notificationBl, out userIds);
                                }
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };
                            }
                            catch (FaultException<BllFnErrorType> ex)
                            {
                                if (ex.Detail != null &&
                                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                                {
                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };
                                }
                                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

                                WebApiApplication.DbLogger.Error(exp);
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(exp.Message, Encoding.UTF8) };
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex);
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(ex.Message, Encoding.UTF8) };
                            }
                        }
                    }
                }
            }
        }
    }
}