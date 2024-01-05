using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.PaymentGateway.Models.IqWallet;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using System.Net;
using System.Text;
using System.ServiceModel;
using System;
using IqSoft.CP.DAL.Models;
using System.Web.Http.Cors;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class IqWalletControllerController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.IqWallet);

        [HttpPost]
        [Route("api/IqWallet/PaymentRequest")]
        public HttpResponseMessage PaymentRequest(PaymentRequestInput input)
        {
            var response = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    try
                    {
                        BaseBll.CheckIp(WhitelistedIps);
                        var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MerchantPaymentId));
                        if (request == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(request.ClientId.Value);
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                            request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                        var signature = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedValuesAsString(input, ",") + partnerPaymentSetting.Password);

                        if (input.Sign.ToLower() != signature.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                        if (input.Status == (int)PaymentRequestStates.Approved)
                        {
                            request.ExternalTransactionId = input.PaymentId.ToString();
                            paymentSystemBl.ChangePaymentRequestDetails(request);
                            clientBl.ApproveDepositFromPaymentSystem(request, false);
                            PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                            BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            response = "OK";
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
                        }
                        else
                        {
                            response = "Error";
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(response, Encoding.UTF8) };
                        }
                    }
                    catch (FaultException<BllFnErrorType> ex)
                    {
                        if (ex.Detail != null &&
                            (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                            ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                        {
                            response = "OK";
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
                        }
                        var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

                        WebApiApplication.DbLogger.Error(exp);
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(exp.Message, Encoding.UTF8) };
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Error(ex);
                    }
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent("", Encoding.UTF8) };
                }
            }
        }

        [HttpPost]
        [Route("api/IqWallet/PayoutRequest")]
        public HttpResponseMessage PayoutRequest(PaymentRequestInput input)
        {
            var response = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var documentBl = new DocumentBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            try
                            {
                                BaseBll.CheckIp(WhitelistedIps);
                                var request = paymentSystemBl.GetPaymentRequestById(System.Convert.ToInt64(input.MerchantPaymentId));
                                if (request == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                if (client == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);

                                var signature = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedValuesAsString(input, ",") + partnerPaymentSetting.Password);

                                if (input.Sign.ToLower() != signature.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                                if (input.Status == (int)PaymentRequestStates.Approved)
                                {
                                    request.ExternalTransactionId = input.PaymentId.ToString();
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, 
                                        string.Empty, request.CashDeskId, null, true, request.Parameters, documentBl, notificationBl);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                    response = "OK";
                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
                                }
                                else
                                {
                                    response = "Error";
                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(response, Encoding.UTF8) };
                                }
                            }
                            catch (FaultException<BllFnErrorType> ex)
                            {
                                if (ex.Detail != null &&
                                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                                {
                                    response = "OK";
                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
                                }
                                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

                                WebApiApplication.DbLogger.Error(exp);
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(exp.Message, Encoding.UTF8) };
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex);
                            }
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent("", Encoding.UTF8) };
                        }
                    }
                }
            }
        }
    }
}
