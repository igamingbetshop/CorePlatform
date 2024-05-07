using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using System.IO;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.KralPay;

namespace IqSoft.CP.PaymentGateway.Controllers
{

    public class KralPayController : ApiController
    {
        private static readonly List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.KralPay);

        [HttpPost]
        [Route("api/KralPay/PaymentInfo")]
        public HttpResponseMessage GetPaymentInfo(PaymentInputBase input)
        {
            var requestOutput = new RequestOutput();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Trx)) ??
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    if (request.ClientId.ToString() != input.UserId)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
                    var client = CacheManager.GetClientById(request.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                       client.CurrencyId, request.Type);
                    if (input.SId != partnerPaymentSetting.UserName || input.MerchantKey != partnerPaymentSetting.Password)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.NotAllowed);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail.Id != Constants.Errors.ClientDocumentAlreadyExists &&
                    ex.Detail.Id != Constants.Errors.RequestAlreadyPayed)
                {
                    requestOutput.Code = "999";
                    requestOutput.Message = ex.Detail.Message;
                }
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                {
                    requestOutput.Code = "999";
                    requestOutput.Message = ex.Message;
                }
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Message);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(requestOutput), Encoding.UTF8) };
        }

        [HttpPost]
        [Route("api/KralPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var requestOutput = new RequestOutput();
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var documentBl = new DocumentBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Trx)) ??
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                       client.CurrencyId, paymentRequest.Type);
                    if (input.SId != partnerPaymentSetting.UserName || input.MerchantKey != partnerPaymentSetting.Password)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.NotAllowed);
                    if (paymentRequest.ClientId.ToString() != input.UserId)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
                    if (input.Amount != paymentRequest.Amount)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
                    if (input.Currency != Constants.Currencies.TurkishLira)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);
                    paymentRequest.ExternalTransactionId = input.TransactionId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                    {
                        if (input.Status == "S")
                            clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                        else if (input.Status == "R")
                            clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Description, notificationBl);
                    }
                    else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                    {
                        if (input.Status == "C")
                        {
                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved,
                            string.Empty, null, null, false, string.Empty, documentBl, notificationBl);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                        }
                        else if (input.Status == "R")
                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Description,
                                                                null, null, false, string.Empty, documentBl, notificationBl);
                    }
                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                    BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail.Id != Constants.Errors.ClientDocumentAlreadyExists &&
                    ex.Detail.Id != Constants.Errors.RequestAlreadyPayed)
                {
                    requestOutput.Code = "999";
                    requestOutput.Message = ex.Detail.Message;
                }
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                {
                    requestOutput.Code = "999";
                    requestOutput.Message = ex.Message;
                }
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Message);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(requestOutput), Encoding.UTF8) };
        }
    }
}