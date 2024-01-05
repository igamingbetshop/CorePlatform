using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Astropay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class AstropayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Astropay);

        [HttpPost]
        [Route("api/Astropay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
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
                            var inputSign = HttpContext.Current.Request.Headers.Get("Signature");
                            WebApiApplication.DbLogger.Info("Signature: " + inputSign);
                            var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MerchantId));
                            if (request == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(request.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                        request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            var sign = CommonFunctions.ComputeHMACSha256(inputString, partnerPaymentSetting.UserName);
                            if (inputSign.ToLower() != sign.ToLower())
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            if (input.Status.ToUpper() == "APPROVED")
                            {
                                clientBl.ApproveDepositFromPaymentSystem(request, false);
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                            else if (input.Status.ToUpper() == "CANCELLED")
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
        [Route("api/Astropay/VerifyRequest")]
        public HttpResponseMessage VerifyPayoutRequest(VerifyPayoutInput input)
        {
            var response = new VerifyPayoutOutput
            {
                ExternalId = input.ExternalId,
                Approve = false
            };
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info(inputString);
                BaseBll.CheckIp(WhitelistedIps);

                using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {                  
                    using (var paymentSystemBl = new PaymentSystemBll(partnerBl))
                    {
                        var inputSign = HttpContext.Current.Request.Headers.Get("Signature");
                        var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MerchantRequestId));
                        if (request == null || request.ExternalTransactionId != input.ExternalId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(request.ClientId.Value);
                        if (client.Id.ToString() != input.ClientId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                   request.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                        if (partnerPaymentSetting == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);

                        var sign = CommonFunctions.ComputeHMACSha256(inputString, partnerPaymentSetting.UserName);
                        if (inputSign.ToLower() != sign.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                        //if (client.CurrencyId != input.Currency)
                        //    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                        //if (decimal.Parse(string.Format("{0:N2}", request.Amount)) != decimal.Parse(string.Format("{0:N2}", input.Amount)))
                        //    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);                     

                        response.Approve = true;
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                WebApiApplication.DbLogger.Error(new Exception(ex.Detail.Message));
                response.Approve = false;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response.Approve = false;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
        }

        [HttpPost]
        [Route("api/Astropay/PayoutRequest")]
        public HttpResponseMessage PayoutRequest(PayoutInput input)
        {
            var response = "OK";
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();

                WebApiApplication.DbLogger.Info(inputString);
                BaseBll.CheckIp(WhitelistedIps);
                var inputSign = HttpContext.Current.Request.Headers.Get("Signature");
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(clientBl))
                            {
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MerchantId));
                                if (request == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                            request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                                var sign = CommonFunctions.ComputeHMACSha256(inputString, partnerPaymentSetting.UserName);
                                if (inputSign.ToLower() != sign.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                                if (input.Status.ToUpper() == "APPROVED")
                                {
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                         null, null, false, string.Empty, documentBll, notificationBl);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                }
                                else if (input.Status.ToUpper() == "CANCELLED")
                                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, 
                                        input.Status, null, null, false, string.Empty, documentBll, notificationBl);
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