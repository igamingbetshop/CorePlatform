using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.OptimumWay;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class OptimumWayController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "??"
        };

        [HttpPost]
        [Route("api/OptimumWay/ApiRequest")]
        public ActionResult ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MerchantTransactionId));
                if (paymentRequest == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                            paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
                var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                parameters.Add("PaymentMethod", input.PaymentMethod);
                paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                {
                    if (input.Result.ToUpper() == "OK")
                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                    else if (input.Result.ToUpper() == "ERROR")
                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted,
                            string.Format("ErrorMessage: {0}, AdapterMessage {1}", input.Message, input.AdapterMessage), notificationBl);
                }
                else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                {
                    using var documentBll = new DocumentBll(paymentSystemBl);
                    if (input.Result.ToUpper() == "OK")
                    {
                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                         null, null, false, string.Empty, documentBll, notificationBl);
                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                    }
                    else if (input.Result.ToUpper() == "ERROR")
                    {
                        var reason = string.Format("ErrorMessage: {0}, AdapterMessage {1}", input.Message, input.AdapterMessage);
                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, reason, null, null, false, 
                                                            string.Empty, documentBll, notificationBl);
                    }
                }
                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
                //BaseHelpers.BroadcastBalance(paymentRequest.ClientId);
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
                Program.DbLogger.Error(response);
                return BadRequest(ex.Message);
            }
        }
    }
}