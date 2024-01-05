using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using System;
using System.Collections.Generic;
using IqSoft.CP.PaymentGateway.Models.Impaya;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class ImpayaController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           ""
        };

        [HttpPost]
        [Route("api/Impaya/ApiRequest")]
        public ActionResult ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);

                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.mc_transaction_id));
                if (paymentRequest == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   client.CurrencyId, paymentRequest.Type);
                var hash = CommonFunctions.ComputeMd5($"{input.transaction_id}{input.amount}{client.CurrencyId}" +
                                                      $"{partnerPaymentSetting.UserName}{input.status_id}{partnerPaymentSetting.Password}");
                if (hash.ToLower() !=  input.hash.ToLower())
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
                if (Convert.ToInt32(input.amount) != Convert.ToInt32(paymentRequest.Amount * 100) || input.currency != paymentRequest.CurrencyId ||
                 (paymentRequest.ExternalTransactionId != null &&  input.transaction_id != paymentRequest.ExternalTransactionId.ToString()))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongInputParameters);
                if (input.status_id == 1)
                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                else if (input.status_id == -1 || input.status_id == 3 || input.status_id == 4 || input.status_id == 5)
                {
                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.payment_system_status, notificationBl);
                }
                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
                //  BaseHelpers.BroadcastBalance(paymentRequest.ClientId);
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
                Program.DbLogger.Error(ex);
            }
            return Ok(response);
        }
    }
}