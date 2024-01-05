using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.PayTrust88;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class PayTrust88Controller : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "52.221.1.215",
            "13.250.0.140",
            "52.220.173.33",
            "13.251.75.191"
        };

        [HttpPost]
        [Route("api/PayTrust88/ApiRequest")]
        public ActionResult ApiRequest(RequestResultInput input)
        {
            var response = string.Empty;
            try
            {
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.item_id));
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.name));
                if (client == null || client.Id.ToString() != input.name)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId, request.CurrencyId,
                                                                                  (int)PaymentRequestTypes.Deposit);
                var signature = string.Format("{0}{1}{2}", input.transaction, input.amount, input.created_at);
                signature = CommonFunctions.ComputeHMACSha256(signature, partnerPaymentSetting.Password).ToLower();
                if (signature != input.signature.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.amount != request.Amount)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                if (input.currency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                if (input.status == 1)//Success
                {
                    var paymentInfo = new PaymentInfo
                    {
                        BankName = input.bank_name,
                        BankAccountNumber = input.bank_account
                    };
                    request.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                    request.ExternalTransactionId = input.transaction.ToString();
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                    response = "State=OK";
                    return Ok(new StringContent(response, Encoding.UTF8));

                }
                else
                {
                    if (input.status == -1 || input.status == -2)
                        clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Failed, input.status_message, notificationBl);
                    response = "State=RETRY";
                    Response.StatusCode = (int)HttpStatusCode.Conflict;
                    return Ok(new StringContent(response, Encoding.UTF8));
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "State=OK";
                    return Ok(new StringContent(response, Encoding.UTF8));
                }
                Program.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
                response = "State=RETRY&ErrorDescription=" + ex.Message;
                return Conflict(new StringContent(response, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = "State=RETRY&ErrorDescription=" + ex.Message;
                return Conflict(new StringContent(response, Encoding.UTF8));
            }
        }

        [HttpPost]
        [Route("api/PayTrust88/PayoutResult")]
        public ActionResult PayoutRequest(PayoutResultInput input)
        {
            var response = string.Empty;
            try
            {
                Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
                {
                    using var clientBl = new ClientBll(paymentSystemBl);
                    using var documentBl = new DocumentBll(paymentSystemBl);
                    using var notificationBl = new NotificationBll(clientBl);
                    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayTrust88);
                    if (paymentSystem == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                    var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.item_id));
                    if (request == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(request.ClientId);
                    if (client == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);

                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, request.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    if (partnerPaymentSetting == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);

                    var signature = string.Format("{0}{1}{2}{3}{4}", input.payout, input.amount, input.currency, input.status, input.created_at);
                    signature = CommonFunctions.ComputeHMACSha256(signature, partnerPaymentSetting.Password);
                    if (signature.ToLower() != input.signature.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);
                    if (input.amount != request.Amount)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                    if (input.currency != client.CurrencyId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);

                    if (input.status >= 0)//success
                    {
                        var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                                                       null, null, false, string.Empty, documentBl, notificationBl);
                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                        PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                        response = "State=OK";
                        return Ok(new StringContent(response, Encoding.UTF8));
                    }
                    else
                    {
                        clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Failed, input.status_message, notificationBl);
                        response = "State=RETRY";
                        return Conflict(new StringContent(response, Encoding.UTF8));
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "State=OK";
                    return Ok(new StringContent(response, Encoding.UTF8));
                }
                Program.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
                response = "State=RETRY&ErrorDescription=" + ex.Message;
                return Conflict(new StringContent(response, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = "State=RETRY&ErrorDescription=" + ex.Message;
                return Conflict(new StringContent(response, Encoding.UTF8));
            }
        }
    }
}