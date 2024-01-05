using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.PaySec;
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
    public class PaySecController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.PaySec);

        [HttpPost]
        [Route("api/PaySec/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentRequestResultInput input)
        {
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    try
                    {
                        BaseBll.CheckIp(WhitelistedIps);
                        var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.cartId));
                        if (request == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                        var client = CacheManager.GetClientById(request.ClientId.Value);
                        var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PaySec);
                        if (paymentSystem == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                           input.currency, (int)PaymentRequestTypes.Deposit);
                        if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                        var merchantCode = partnerPaymentSetting.UserName;
                        var merchantKey = partnerPaymentSetting.Password;

                        var signature = string.Format("{0};{1};{2};{3};{4};{5}",
                                                       input.cartId, input.orderAmount,
                                                       input.currency, merchantCode,
                                                       input.version, input.status);
                        signature = CommonFunctions.ComputeSha256(signature);
                        signature = BCrypt.Net.BCrypt.HashPassword(signature, merchantKey).Replace(merchantKey, string.Empty);

                        if (signature.ToLower() != input.signature.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);

                        if (input.status.ToLower() == "success")
                        {
                            request.ExternalTransactionId = input.transactionReference;
                            paymentSystemBl.ChangePaymentRequestDetails(request);
							clientBl.ApproveDepositFromPaymentSystem(request, false);
                            PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                            BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };
                        }
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent("FAILED", Encoding.UTF8) };
                    }
                    catch (FaultException<BllFnErrorType> ex)
                    {
                        if (ex.Detail != null &&
                            (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                            ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                        {
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };
                        }
                        WebApiApplication.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
                        var response = "State=FAILED&ErrorDescription=" + ex.Message;
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(response, Encoding.UTF8) };
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Error(ex);
                        var response = "State=FAILED&ErrorDescription=" + ex.Message;
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(response, Encoding.UTF8) };
                    }
                }
            }
        }

        [HttpPost]
        [Route("api/PaySec/PayoutResult")]
        public HttpResponseMessage PayoutRequest(PayoutResultInput input)
        {
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBl = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(clientBl))
                            {
                                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PaySec);
                                if (paymentSystem == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.CartId));
                                if (request == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);

                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, request.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                                if (partnerPaymentSetting == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);

                                var merchantCode = partnerPaymentSetting.UserName;
                                var merchantKey = partnerPaymentSetting.Password;
                                var signature = string.Format("{0};{1};{2};{3};{4};{5}",
                                                               input.CartId, input.OrderAmount,
                                                               input.Currency, merchantCode,
                                                               input.Version, input.Status);
                                signature = CommonFunctions.ComputeSha256(signature);
                                signature = BCrypt.Net.BCrypt.HashPassword(signature, merchantKey).Replace(merchantKey, string.Empty);

                                if (signature.ToLower() != input.Signature.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);

                                if (input.Status.ToLower() == "success")
                                {
                                    request.ExternalTransactionId = input.TransactionReference;
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, 
                                        string.Empty, request.CashDeskId, null, true, request.Parameters, documentBl, notificationBl);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };
                                }
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent("FAILED", Encoding.UTF8) };
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
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };
                }
                WebApiApplication.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
                var response = "State=FAILED&ErrorDescription=" + ex.Message;
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(response, Encoding.UTF8) };
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = "State=FAILED&ErrorDescription=" + ex.Message;
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(response, Encoding.UTF8) };
            }
        }
	}
}

