using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Azulpay;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;

namespace IqSoft.CP.PaymentGatewayCoreControllers
{

    [ApiController]
    public class AzulpayController : ControllerBase
    {
        [HttpPost]
        [Route("api/Azulpay/ApiRequest")]
        public ActionResult ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
            try
            {
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);


                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.CustomerOrderId));
                if (paymentRequest == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                if (input.TransactionStatus == "APPROVED")
                {
                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                }
                else if (input.TransactionStatus == "DECLINED" || input.TransactionStatus == "ERROR")
                {
                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
                        $"Status: {input.TransactionStatus} Reason: {input.Reason}", notificationBl);
                }
                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
                //BaseHelpers.BroadcastBalance(paymentRequest.ClientId);
                return Ok(new StringContent("State=OK", Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null)
                {
                    if (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists || ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)
                        response = "OK";
                    else if (ex.Detail.Id == Constants.Errors.WrongHash)
                    {
                        response = ex.Detail.Id + " " + ex.Detail.NickName;
                    }
                }
                else
                {
                    response = ex.Message;
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