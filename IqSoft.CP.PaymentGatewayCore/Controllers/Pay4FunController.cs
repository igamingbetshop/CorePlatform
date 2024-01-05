using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.PaymentGateway.Models.Pay4Fun;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using System.ServiceModel;
using System;
using IqSoft.CP.PaymentGateway.Helpers;
using Newtonsoft.Json;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class Pay4FunController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "52.55.31.44",//staging
           "34.204.211.133",
           "34.196.30.64",
           "3.211.164.78",
           "3.231.62.35",
        };

        [HttpPost]
        [Route("api/Pay4Fun/ApiRequest")]
        public ActionResult ApiRequest(PaymentInput input)
        {
            var response = "Failed";
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                //  BaseBll.CheckIp(WhitelistedIps, ip);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);

                var request = paymentSystemBl.GetPaymentRequestById(input.MerchantInvoiceId);
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                            request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var merchantKeys = partnerPaymentSetting.Password.Split(',');
                var sign = string.Format("{0}{1}{2}{3}", partnerPaymentSetting.UserName, Math.Truncate(input.Amount * 100), request.Id, input.Status);
                sign = CommonFunctions.ComputeHMACSha256(sign, merchantKeys[0]);
                if (sign.ToLower() != input.Sign.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.Status == "201")
                    clientBl.ApproveDepositFromPaymentSystem(request, false, comment: input.Message);
                else
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.Message, notificationBl);
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
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Message);
            }
            Response.ContentType = Constants.HttpContentTypes.ApplicationJson;
            return Ok(response);
        }
    }
}