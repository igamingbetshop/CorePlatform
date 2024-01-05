﻿using System;
using System.Collections.Generic;
using IqSoft.CP.PaymentGateway.Models.PayOp;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using System.IO;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class PayOpController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "52.49.204.201",
           "54.229.170.212"
        };
        private enum Statuses
        {
            New = 0,
            Accepted = 1,
            Pending = 4,
            Failed = 5
        }

        [HttpPost]
        [Route("api/PayOp/PaymentRequest")]
        public HttpResponseMessage PaymentRequest(PaymentInput input)
        {
            var response = "Success";
            try
            {
                //  BaseBll.CheckIp(WhitelistedIps);

                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt32(input.TransactionDetails.OrderDetails.Id));
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                request.ExternalTransactionId = input.TransactionDetails.Id;
                paymentSystemBl.ChangePaymentRequestDetails(request);

                if (input.InvoiceDetails.Status == (int)Statuses.Accepted)
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                else if (input.InvoiceDetails.Status == (int)Statuses.Failed)
                {
                    var desc = string.Format("Code: {0}, Message: {1}", input.TransactionDetails.ErrorDetails.Code, input.TransactionDetails.ErrorDetails.Message);
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
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }
    }
}