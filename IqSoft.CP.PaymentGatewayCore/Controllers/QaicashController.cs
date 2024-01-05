using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.PaymentGateway.Models.Qaicash;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using System.ServiceModel;
using System;
using System.Text;
using Newtonsoft.Json;
using IqSoft.CP.PaymentGateway.Helpers;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class QaicashController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "52.220.81.104",
           "52.76.237.60",
           "104.155.236.244",
           "35.229.236.182",
           "35.221.219.2"
        };

        [HttpPost]
        [Route("api/Qaicash/ApiRequest")]
        public ActionResult ApiRequest(PaymentInput input)
        {
            var response = "Failed";
            try
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(input.OrderId);
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                            request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var messageAuthenticationCode = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", input.OrderId, input.TransactionId, input.DateCreated,
                 input.DepositMethod, input.Amount, client.CurrencyId, input.Status, input.DateUpdated, input.DepositorUserId).Replace("||", "|");
                messageAuthenticationCode = CommonFunctions.ComputeHMACSha256(messageAuthenticationCode, partnerPaymentSetting.Password).ToLower();

                if (messageAuthenticationCode != input.MessageAuthenticationCode.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var amount = Convert.ToDecimal(input.Amount);
                if (amount != request.Amount)
                {
                    var parameters = string.IsNullOrEmpty(request.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
                    parameters.Add("BeforeAmount", request.Amount.ToString());
                    parameters.Add("AdjustmentAmount", amount.ToString());
                    request.Parameters = JsonConvert.SerializeObject(parameters);
                    request.Amount = amount;
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                }

                if (input.Status.ToUpper() == "SUCCESS")
                    clientBl.ApproveDepositFromPaymentSystem(request, false, comment: input.Status);
                else if (input.Status.ToUpper() == "FAILED")
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.Status, notificationBl);
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
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                Program.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                Program.DbLogger.Error(response);
            }
            return Ok(new StringContent(response, Encoding.UTF8));
            
        }

        [HttpPost]
        [Route("api/Qaicash/PayoutRequest")]
        public ActionResult PayPaymentRequest(PayoutInput input)
        {
            string response;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
                using var paymentSystemBl = new PaymentSystemBll(clientBl);
                using var documentBl = new DocumentBll(clientBl);
                using var notificationBl = new NotificationBll(clientBl);
                var request = paymentSystemBl.GetPaymentRequestById(input.OrderId);
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var messageAuthenticationCode = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}", input.OrderId, input.TransactionId,
                                                input.DateCreated, input.Amount, client.CurrencyId, input.Status,
                                                input.DateUpdated, input.UserId).Replace("||", "|");
                messageAuthenticationCode = CommonFunctions.ComputeHMACSha256(messageAuthenticationCode, partnerPaymentSetting.Password).ToLower();

                if (messageAuthenticationCode != input.MessageAuthenticationCode.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                request.ExternalTransactionId = input.TransactionId;
                paymentSystemBl.ChangePaymentRequestDetails(request);
                if (input.Status.ToUpper() == "SUCCESS")
                {
                    var req = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, input.InstrumentId.ToString(),
                                                                  null, null, false, string.Empty, documentBl, notificationBl);
                    clientBl.PayWithdrawFromPaymentSystem(req, documentBl, notificationBl);
                }
                else if (input.Status.ToUpper() == "FAILED")
                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, input.Notes,
                                                        null, null, false, string.Empty, documentBl, notificationBl);
                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                return Ok(new StringContent("OK", Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                    return Ok(new StringContent("OK", Encoding.UTF8));

                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
                response = exp.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex);
                response = ex.Message;
            }
            return Ok(new StringContent(response, Encoding.UTF8));
            
        }
    }
}
