using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.NodaPay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET,POST")]
    public class NodaPayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.NodaPay);

        [HttpPost]
        [Route("api/NodaPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentRequest input)
        {
            var response = string.Empty;
            var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
            var inputString = bodyStream.ReadToEnd();
            WebApiApplication.DbLogger.Info(inputString);
            try
            {
                //  BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MerchantPaymentId)) ??
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                            var client = CacheManager.GetClientById(request.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                        request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                            var sign = CommonFunctions.ComputeSha256(input.PaymentId + input.Status + partnerPaymentSetting.Password.Split(',')[1]);
                            if (input.Signature.ToLower() != sign.ToLower())
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                            if (input.Status.ToLower() == "done")
                            {
                                clientBl.ApproveDepositFromPaymentSystem(request, false);
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                            else if (input.Status.ToLower() == "failed")
                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.Reference, notificationBl);
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
        [Route("api/NodaPay/PayoutRequest")]
        public HttpResponseMessage PayoutRequest(PayoutInput input)
        {
            var response = "OK";
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(clientBl))
                            {
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.RequestId)) ??
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                                var sign = CommonFunctions.ComputeSha256(input.PaymentId + input.Status + partnerPaymentSetting.Password);
                                if (sign.ToLower() != input.Signature.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                                if (input.Status.ToLower() == "done")
                                {
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, input.Status,
                                         null, null, false, request.Parameters, documentBll, notificationBl);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                }
                                else if (input.Status.ToLower() == "failed")
                                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed,
                                        input.Status, null, null, false, request.Parameters, documentBll, notificationBl);
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
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