using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using System.ServiceModel;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.PaymentGateway.Models.Eway;
using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class EwayController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "??" //distribution 
        };

        [HttpGet]
        [Route("api/Eway/ApiRequest")]
        public ActionResult ApiRequest([FromQuery]int transactionId)
        {
            var response = string.Empty;
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(transactionId);
                if (paymentRequest == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                if (paymentSystem.Name != Constants.PaymentSystems.Eway)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.EwayApiUrl);
                var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                if (!parameters.ContainsKey("AccessCode"))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);
                var byteArray = Encoding.Default.GetBytes($"{partnerPaymentSetting.Password}:{partnerPaymentSetting.UserName}");
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpMethod.Get,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(byteArray) } },
                    Url = string.Format("{0}/AccessCode/{1}", url, parameters["AccessCode"])
                };
                var result = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                Program.DbLogger.Info("Payment result: " +result);
                var paymentResultOutput = JsonConvert.DeserializeObject<PaymentResultOutput>(result);
                if (string.IsNullOrEmpty(paymentResultOutput.TransactionID) || paymentResultOutput.ResponseCode != "00")
                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, paymentResultOutput.ResponseMessage, notificationBl);
                else if (paymentResultOutput.TransactionStatus)
                {
                    paymentRequest.ExternalTransactionId = paymentResultOutput.TransactionID;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                }
                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
                //BaseHelpers.BroadcastBalance(paymentRequest.ClientId);
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