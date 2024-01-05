using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Models.CashLib;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class CashLibController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           ""
        };

        [HttpPost]
        [Route("api/CashLib/ApiRequest")]
        public ActionResult ApiRequest(PaymentInput input)
        {
            var response = "Success";
            try
            {

                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(input.MerchantTransactionId);
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                request.ExternalTransactionId = input.TransactionId;
                paymentSystemBl.ChangePaymentRequestDetails(request);

                if (input.Status == 0)
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                else
                {
                    var desc = string.Format("Code: {0}, Message: {1}", input.Status, input.ErrorMessage);
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, desc, notificationBl);
                }
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

        [HttpGet]
        [Route("api/CashLib/CancelRequest")]
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

                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, "Transaction is unfinished from provider's frame", notificationBl);
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