using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Interac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;


namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class InteracController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "3.14.140.68",
           "3.137.104.161",
           "3.139.184.24",
           "18.220.71.87",
           "3.22.210.96",
           "3.21.110.128",
           "3.128.39.136",
           "18.222.52.79",
           "184.71.40.34"
        };

        [HttpPost]
        [Route("api/Interac/ApiRequest")]
        public ActionResult ApiRequest([FromBody]PaymentInput input, [FromQuery]string status)
        {
            var response = string.Empty;
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TransactionId));
                if (paymentRequest == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                            paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
                if (input.Amount != paymentRequest.Amount || input.Currency.ToUpper()!= paymentRequest.CurrencyId || client.Id.ToString() != input.UserId)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongInputParameters);
                if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                {
                    if (status == "STATUS_SUCCESS")
                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                    else if (status == "STATUS_ABORTED1" || status == "	STATUS_ABORTED" || status == "STATUS_FAILED" ||
                            status == "STATUS_EXPIRED" || status == "STATUS_REJECTED")
                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, status, notificationBl);
                }
                else
                {
                    using (var documentBll = new DocumentBll(paymentSystemBl))
                    {
                        if (status == "STATUS_SUCCESS")
                        {
                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                              null, null, false, string.Empty, documentBll, notificationBl);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                        }
                        else if (status == "STATUS_REJECTED" || status == "	STATUS_EXPIRED")
                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, status, null, null, false, 
                                                                string.Empty, documentBll, notificationBl);
                    }
                }
                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
                //  BaseHelpers.BroadcastBalance(paymentRequest.ClientId);
                return Ok("OK");
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
                {
                    response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                    httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                }
                Program.DbLogger.Error(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                Program.DbLogger.Error(response);
                return BadRequest(response);
            }
        }
    }
}