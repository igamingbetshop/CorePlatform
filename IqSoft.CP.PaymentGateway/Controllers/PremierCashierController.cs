using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using IqSoft.CP.PaymentGateway.Models.PremierCashier;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class PremierCashierController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.PremierCashier);

        [HttpPost]
        [Route("api/PremierCashier/InitRequest")]
        public HttpResponseMessage InitRequest(HttpRequestMessage httpRequestMessage)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            WebApiApplication.DbLogger.Info(inputString);
            var orderId = string.Empty;
            var responseString = string.Empty;
            try
            {
                var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
                orderId = input.Crm.OrderId;
                //BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.Contains("Cashier-Signature"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var signature = Request.Headers.GetValues("Cashier-Signature").FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Crm.OrderId)) ??
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value) ??
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                       client.CurrencyId, paymentRequest.Type);
                    var frontendTokens = partnerPaymentSetting.UserName.Split(',');
                    if (CommonFunctions.ComputeHMACSha384(inputString, partnerPaymentSetting.Password).ToLower() != signature)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    if (input.FrontendId.ToString() != frontendTokens[0] || input.Tokenname != frontendTokens[1])
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                    if (client.Id.ToString() != input.Pin)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                    if (input.Transaction.WalletDetails != null)
                    {
                        paymentInfo.WalletNumber = input.Transaction.WalletDetails?.AccountIdentifier;
                        paymentInfo.AccountType = input.Transaction.WalletDetails?.PaymentMethod;
                    }
                    else if (input.Transaction.CardDetails != null)
                    {
                        paymentInfo.CardNumber = input.Transaction.CardDetails.CardNumberMasked;
                        paymentInfo.CardHolderName = input.Transaction.CardDetails.CardHolderName;
                        paymentInfo.CardType = input.Transaction.CardDetails.PaymentMethod;
                        paymentInfo.ExpirationDate = input.Transaction.CardDetails.CardExp;
                    }
                    if (client.State == (int)ClientStates.FullBlocked ||client.State == (int)ClientStates.BlockedForDeposit ||
                        client.State == (int)ClientStates.Suspended || client.State == (int)ClientStates.Disabled)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    paymentInfo.Info = JsonConvert.SerializeObject(paymentInfo);
                    if (!string.IsNullOrEmpty(input.Transaction.TraceId))
                        paymentRequest.ExternalTransactionId = input.Transaction.TraceId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    if (input.Transaction.Amount != paymentRequest.Amount)
                    {
                        if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                        paymentRequest.Amount = input.Transaction.Amount;
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    }
                    responseString = JsonConvert.SerializeObject(
                        new
                        {
                            code = "0",
                            message = "success",
                            order_id = paymentRequest.Id.ToString()
                        });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                responseString = JsonConvert.SerializeObject(
                new
                {
                    code = ex.Detail.Id.ToString(),
                    message = ex.Detail.Message,
                    order_id = orderId
                });

                WebApiApplication.DbLogger.Error("InputString: " + inputString + "__Response: " +  responseString+ "__Exc: " + ex.Detail);
            }
            catch (Exception ex)
            {
                responseString = JsonConvert.SerializeObject(
                new
                {
                    code = Constants.Errors.GeneralException.ToString(),
                    message = ex.Message,
                    order_id = orderId
                });

                WebApiApplication.DbLogger.Error("InputString: " + inputString + "__Response: " +  responseString+ "__Exc: " + ex);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseString, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("api/PremierCashier/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            WebApiApplication.DbLogger.Info(inputString);
            var orderId = string.Empty;
            var responseString = string.Empty;
            try
            {
                var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
                orderId = input.Crm.OrderId;
                //  BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.Contains("Cashier-Signature"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var signature = Request.Headers.GetValues("Cashier-Signature").FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Crm.OrderId)) ??
                                 throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value) ??
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                               client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                            var frontendTokens = partnerPaymentSetting.UserName.Split(',');
                            if (CommonFunctions.ComputeHMACSha384(inputString, partnerPaymentSetting.Password).ToLower() != signature)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            if (input.FrontendId.ToString() != frontendTokens[0] || input.Tokenname != frontendTokens[1])
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                            if (input.Transaction.Amount != paymentRequest.Amount)
                            {
                                if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                                paymentRequest.Amount = input.Transaction.Amount;
                                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            }
                            paymentRequest.ExternalTransactionId = input.Transaction.TraceId;
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            if (input.Transaction.Status == "approved" || input.Transaction.Status == "processed")
                            {
                                if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                                    using (var documentBll = new DocumentBll(paymentSystemBl))
                                    {
                                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                                                                       null, null, false, string.Empty, documentBll, notificationBl);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    }
                                else
                                {
                                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, comment: input.Processor.StatusMessage);
                                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                                }
                            }
                            else if (input.Transaction.Status == "canceled" || input.Transaction.Status == "rejected")
                            {
                                if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Processor.StatusMessage, notificationBl);
                                else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                                    using (var documentBll = new DocumentBll(paymentSystemBl))
                                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Processor.StatusMessage, null, null, false, string.Empty, documentBll, notificationBl);
                            }
                            responseString = JsonConvert.SerializeObject(
                                new
                                {
                                    code = "0",
                                    message = "success",
                                    order_id = paymentRequest.Id.ToString()
                                });
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                responseString = JsonConvert.SerializeObject(
                new
                {
                    code = ex.Detail.Id.ToString(),
                    message = ex.Detail.Message,
                    order_id = orderId
                });

                WebApiApplication.DbLogger.Error("InputString: " + inputString + "__Response: " +  responseString+ "__Exc: " + ex.Detail);
            }
            catch (Exception ex)
            {
                responseString = JsonConvert.SerializeObject(
                new
                {
                    code = Constants.Errors.GeneralException.ToString(),
                    message = ex.Message,
                    order_id = orderId
                });

                WebApiApplication.DbLogger.Error("InputString: " + inputString + "__Response: " +  responseString+ "__Exc: " + ex);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseString, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }


        [HttpPost]
        [Route("api/PremierCashier/GetBalance")]
        public HttpResponseMessage GetBalance(HttpRequestMessage httpRequestMessage)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            WebApiApplication.DbLogger.Info(inputString);
            var responseString = string.Empty;
            try
            {
                var input = JsonConvert.DeserializeObject<BaseInput>(inputString);
                // BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.Contains("Cashier-Signature"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var signature = Request.Headers.GetValues("Cashier-Signature").FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.Pin)) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (client.CurrencyId != input.CurrencyCode)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    responseString = JsonConvert.SerializeObject(
                    new
                    {
                        code = "0",
                        message = "success",
                        available_balance = paymentSystemBl.GetClientLastWithdrawRequest(client.Id)?.Amount
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                responseString = JsonConvert.SerializeObject(
                new
                {
                    code = ex.Detail.Id.ToString(),
                    message = ex.Detail.Message,
                    available_balance = 0
                });

                WebApiApplication.DbLogger.Error("InputString: " + inputString + "__Response: " +  responseString+ "__Exc: " + ex.Detail);
            }
            catch (Exception ex)
            {
                responseString = JsonConvert.SerializeObject(
                new
                {
                    code = Constants.Errors.GeneralException.ToString(),
                    message = ex.Message,
                    available_balance = 0
                });

                WebApiApplication.DbLogger.Error("InputString: " + inputString + "__Response: " +  responseString+ "__Exc: " + ex);
            }
            WebApiApplication.DbLogger.Debug(responseString);
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseString, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

    }
}