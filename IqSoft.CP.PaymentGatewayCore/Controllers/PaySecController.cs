using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.PaySec;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class PaySecController : ControllerBase
    {
		private static readonly List<string> WhitelistedIps = new List<string> { "52.221.164.126" };

        [HttpPost]
        [Route("api/PaySec/ApiRequest")]
        public ActionResult ApiRequest(PaymentRequestResultInput input)
        {
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    try
                    {
                        var ip = string.Empty;
                        if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                            ip = header.ToString();
                        BaseBll.CheckIp(WhitelistedIps, ip);
                        var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.cartId));
                        if (request == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                        var client = CacheManager.GetClientById(request.ClientId);
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
                          
                            return Ok(new StringContent("OK", Encoding.UTF8));
                          
                        }
                        return Conflict(new StringContent("FAILED", Encoding.UTF8));
                       
                    }
                    catch (FaultException<BllFnErrorType> ex)
                    {
                        if (ex.Detail != null &&
                            (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                            ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                        {
                            return Ok(new StringContent("OK", Encoding.UTF8));
                        }
                        Program.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
                        var response = "State=FAILED&ErrorDescription=" + ex.Message;
                        return Conflict(new StringContent(response, Encoding.UTF8));
                       
                    }
                    catch (Exception ex)
                    {
                        Program.DbLogger.Error(ex);
                        var response = "State=FAILED&ErrorDescription=" + ex.Message;
                        return Conflict(new StringContent(response, Encoding.UTF8));
                        
                    }
                }
            }
        }

        [HttpPost]
        [Route("api/PaySec/PayoutResult")]
        public ActionResult PayoutRequest(PayoutResultInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var documentBl = new DocumentBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(clientBl);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PaySec);
                if (paymentSystem == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.CartId));
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);

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
                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                                                   request.CashDeskId, null, true, request.Parameters, documentBl, notificationBl);
                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                    return Ok(new StringContent("OK", Encoding.UTF8));
                }
                return Conflict(new StringContent("FAILED", Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    return Ok(new StringContent("OK", Encoding.UTF8));
                }
                Program.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
                var response = "State=FAILED&ErrorDescription=" + ex.Message;
                return Conflict(new StringContent(response, Encoding.UTF8));

            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = "State=FAILED&ErrorDescription=" + ex.Message;
                return Conflict(new StringContent(response, Encoding.UTF8));

            }
        }
	}
}

