using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "" //distribution
        };
        [HttpGet]
        [Route("api/Payment/CancelRequest")]
        public ActionResult CancelRequest([FromQuery] string paymentRequestId)
        {
            var response = "Success";
            try
            {
                //  BaseBll.CheckIp(WhitelistedIps);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt32(paymentRequestId));
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.CanceledByClient, "Canceled by customer", notificationBl);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "Success";
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