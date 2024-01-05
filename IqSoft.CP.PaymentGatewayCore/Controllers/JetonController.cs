using System;
using System.Collections.Generic;
using System.Net.Http;
using IqSoft.CP.PaymentGateway.Models.Jeton;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.Common.Helpers;
using Microsoft.Extensions.Primitives;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class JetonController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "54.217.102.135",
           "108.129.35.181",
           "52.48.248.88",
           "54.78.9.129",
           "63.35.98.151",
           "54.72.34.239"
        };

        [HttpPost]
        [Route("api/Jeton/ApiRequest")]
        public ActionResult ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(input.OrderId);
                if (paymentRequest == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                            paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
                if (paymentRequest.Amount != input.Amount)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);
                if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit && input.Type.ToUpper() == "PAY")
                {
                    if (input.Status.ToUpper() == "SUCCESS")
                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                    else if (input.Status.ToUpper() == "ERROR") // to check
                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Message, notificationBl);
                }
                else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw && input.Type.ToUpper() == "PAYOUT")
                {
                    using (var documentBll = new DocumentBll(paymentSystemBl))
                    {
                        if (input.Status.ToUpper() == "SUCCESS")
                        {
                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                             null, null, false, string.Empty, documentBll, notificationBl);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                        }
                        else if (input.Status.ToUpper() == "ERROR") // to check statuses
                        {
                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Message, null, null,
                                                                false, string.Empty, documentBll, notificationBl);
                        }
                    }
                }
                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
                // BaseHelpers.BroadcastBalance(paymentRequest.ClientId);
                response = "OK";
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "OK";
                }
                else
                    response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                Program.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                Program.DbLogger.Error(response);
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("api/Jeton/CashRequest")]
        public ActionResult CashRequest(CashInput input)
        {
            var response = string.Empty;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt32(input.ReferenceNo));
                if (paymentRequest == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                            paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
                var hash = CommonFunctions.ComputeMd5($"{partnerPaymentSetting.Password}.{paymentRequest.Id}." +
                                                      $"{input.PaymentMethod}.{input.Amount}.{client.CurrencyId}");
                if (paymentRequest.Amount != Convert.ToDecimal(input.Amount))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
                if (client.CurrencyId != input.Currency)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);
                if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit && input.PaymentMethod == "REDEEM_VOUCHER")
                {
                    if (input.Status.ToUpper() == "APPROVED")
                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                    else if (input.Status.ToUpper() == "DECLINED")
                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Message, notificationBl);
                }
                else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw /*&& input.PaymentMethod == "REDEEM_VOUCHER"*/)
                {
                    using (var documentBll = new DocumentBll(paymentSystemBl))
                    {
                        if (input.Status.ToUpper() == "APPROVED")
                        {
                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                             null, null, false, string.Empty, documentBll, notificationBl);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                        }
                        else if (input.Status.ToUpper() == "DECLINED")
                        {
                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
                                input.Message, null, null, false, string.Empty, documentBll, notificationBl);
                        }
                    }
                }
                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
                //BaseHelpers.BroadcastBalance(paymentRequest.ClientId);
                response = JsonConvert.SerializeObject(new
                {
                    token = CommonFunctions.ComputeMd5($"{partnerPaymentSetting.Password}." +
                                                       $"{paymentRequest.Id}.{input.PaymentMethod}")
                });
                response = "OK";
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "OK";
                }
                else
                    response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                Program.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                Program.DbLogger.Error(response);
            }
            return Ok(response);
        }
    }
}