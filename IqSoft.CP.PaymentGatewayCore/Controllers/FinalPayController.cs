using System;
using System.Collections.Generic;
using System.ServiceModel;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.FinalPay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class FinalPayController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "??"
        };

        [HttpPost]
        [Route("api/FinalPay/ApiRequest")]
        public ActionResult ApiRequest(PaymentRequestInput input)
        {
            var response = string.Empty;
            try
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                //BaseBll.CheckIp(WhitelistedIps, ip);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.DataDetails.OrderId));
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                   client.CurrencyId, request.Type);
                var checkSum = Integration.Payments.Helpers.FinalPayHelpers.ConcatParams(input.DataDetails);
                if (CommonFunctions.ComputeSha256(checkSum + partnerPaymentSetting.Password).ToLower() !=  input.CheckSum.ToLower())
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
                if (input.DataDetails.Code == 0 && input.DataDetails.Status.ToUpper() == "CAPTURED")
                {
                    if (request.Type == (int)PaymentRequestTypes.Deposit)
                        clientBl.ApproveDepositFromPaymentSystem(request, false);
                    else if (request.Type == (int)PaymentRequestTypes.Withdraw)
                    {
                        using var documentBll = new DocumentBll(paymentSystemBl);
                        using var notificationBl = new NotificationBll(clientBl);

                        var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                                                       null, null, false, request.Parameters, documentBll, notificationBl);
                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                    }
                }
                else if (input.DataDetails.Status.ToUpper() == "DECLINED" || input.DataDetails.Status.ToUpper() == "CANCELLED" ||
                         input.DataDetails.Status.ToUpper() == "REJECTED" || input.DataDetails.Status.ToUpper() == "ERROR")
                {
                    using var documentBll = new DocumentBll(paymentSystemBl);
                    using var notificationBl = new NotificationBll(paymentSystemBl);
                    if (request.Type == (int)PaymentRequestTypes.Deposit)
                        clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.DataDetails.Message, notificationBl);
                    else if (request.Type == (int)PaymentRequestTypes.Withdraw)
                        clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, input.DataDetails.Message, null,
                                                            null, false, request.Parameters, documentBll, notificationBl);
                }
                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
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
            return Ok(response);
        }
    }
}