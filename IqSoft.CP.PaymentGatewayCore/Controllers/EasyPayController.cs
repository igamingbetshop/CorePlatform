using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Models.EasyPay;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using IqSoft.CP.PaymentGateway.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class EasyPayController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "35.246.248.42"
        };

        [HttpPost]
        [Route("api/EasyPay/ApiRequest")]
        public ActionResult ApiRequest(HttpRequestMessage input)
        {
            try
            {
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);

                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var inputString = input.Content.ReadAsStringAsync();
                var serializer = new XmlSerializer(typeof(payfrexresponse), new XmlRootAttribute("payfrex-response"));
                var inputObject = (payfrexresponse)serializer.Deserialize(new StringReader(inputString.Result));
                Program.DbLogger.Info(inputString.Result);

                var request = paymentSystemBl.GetPaymentRequestById(inputObject.operations.operation.merchantTransactionId);
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                request.ExternalTransactionId = inputObject.operations.operation.payFrexTransactionId.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(request);
                if (!string.IsNullOrEmpty(inputObject.status) && inputObject.status.ToUpper() == "SUCCESS" &&
                    !string.IsNullOrEmpty(inputObject.operations.operation.status) && inputObject.operations.operation.status.ToUpper() == "SUCCESS")
                {
                    var pInfo = PaymentHelpers.RegisterClientPaymentAccountDetails(new DAL.ClientPaymentInfo
                    {
                        Type = (int)ClientPaymentInfoTypes.CreditCard,
                        CardNumber = inputObject.operations.operation.paymentDetails.cardNumber,
                        ClientFullName = inputObject.operations.operation.paymentDetails.cardHolderName,
                        WalletNumber = inputObject.operations.operation.paymentDetails.cardNumberToken,
                        PartnerPaymentSystemId = request.PartnerPaymentSettingId,
                        CreationTime = request.CreationTime,
                        LastUpdateTime = request.LastUpdateTime,
                        ClientId = request.ClientId,
                        AccountNickName = Constants.PaymentSystems.EasyPayCard
                    });
                    clientBl.ApproveDepositFromPaymentSystem(request, false, string.Empty, pInfo);
                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                }
                else if ((!string.IsNullOrEmpty(inputObject.status) && (inputObject.status.ToUpper() == "FAIL" || inputObject.status.ToUpper() == "ERROR")) ||
                         (!string.IsNullOrEmpty(inputObject.operations.operation.status) &&
                         (inputObject.operations.operation.status.ToUpper() == "FAIL" || inputObject.operations.operation.status.ToUpper() == "ERROR")))
                {
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, inputObject.operations.operation.message, notificationBl);
                }
                return Ok(new StringContent("OK", Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {

                    return Ok(new StringContent("OK", Encoding.UTF8));
                }
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

                Program.DbLogger.Error(exp);
                Response.StatusCode = (int)HttpStatusCode.Conflict;
                return Ok(new StringContent(ex.Message, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                Response.StatusCode = (int)HttpStatusCode.Conflict;
                return Ok(new StringContent(ex.Message, Encoding.UTF8));
            }
        }

        [HttpPost]
        [Route("api/EasyPay/PayoutRequest")]
        public ActionResult PayoutRequest(HttpRequestMessage input)
        {
            using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
            using var clientBl = new ClientBll(paymentSystemBl);
            using var documentBl = new DocumentBll(paymentSystemBl);
            using var notificationBl = new NotificationBll(paymentSystemBl);
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var inputString = input.Content.ReadAsStringAsync();
                var serializer = new XmlSerializer(typeof(payfrexresponse), new XmlRootAttribute("payfrex-response"));
                var inputObjcet = (payfrexresponse)serializer.Deserialize(new StringReader(inputString.Result));
                Program.DbLogger.Info(inputString.Result);

                var request = paymentSystemBl.GetPaymentRequestById(inputObjcet.operations.operation.merchantTransactionId);
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                request.ExternalTransactionId = inputObjcet.operations.operation.payFrexTransactionId.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(request);
                if ((!string.IsNullOrEmpty(inputObjcet.status) && inputObjcet.status.ToUpper() == "SUCCESS") &&
                    (!string.IsNullOrEmpty(inputObjcet.operations.operation.status) && inputObjcet.operations.operation.status.ToUpper() == "SUCCESS"))
                {
                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                               null, null, false, string.Empty, documentBl, notificationBl);
                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                }
                else if ((!string.IsNullOrEmpty(inputObjcet.status) && (inputObjcet.status.ToUpper() == "FAIL" || inputObjcet.status.ToUpper() == "ERROR")) ||
                         (!string.IsNullOrEmpty(inputObjcet.operations.operation.status) &&
                         (inputObjcet.operations.operation.status.ToUpper() == "FAIL" || inputObjcet.operations.operation.status.ToUpper() == "ERROR")))
                {
                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, inputObjcet.operations.operation.message, null, 
                                                        null, false, string.Empty, documentBl, notificationBl);
                }
                return Ok(new StringContent("OK", Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    return Ok(new StringContent("OK", Encoding.UTF8));
                }
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

                Program.DbLogger.Error(exp);
                Response.StatusCode = (int)HttpStatusCode.Conflict;
                return Ok(new StringContent(exp.Message, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                Response.StatusCode = (int)HttpStatusCode.Conflict;
                return Ok(new StringContent(ex.Message, Encoding.UTF8));
            }
        }
    }
}