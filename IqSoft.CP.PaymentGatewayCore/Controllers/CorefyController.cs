using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Net.Http.Headers;
using IqSoft.CP.PaymentGateway.Models.Corefy;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class CorefyController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           ""
        };

        [HttpPost]
        [Route("api/Corefy/ApiRequest")]
        public ActionResult ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var response = string.Empty;
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
                if (!Request.Headers.ContainsKey("X-Signature"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var signature = Request.Headers["X-Signature"];
                if (string.IsNullOrEmpty(signature))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                if (string.IsNullOrEmpty(signature))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                using var documentBl = new DocumentBll(paymentSystemBl);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Data.Attributes.ReferenceId));
                if (paymentRequest == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   client.CurrencyId, paymentRequest.Type);
                var sign = ComputeSha1($"{partnerPaymentSetting.Password}{inputString}{partnerPaymentSetting.Password}");
                if (sign.ToLower() != signature.ToString().ToLower())
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);

                if (input.Data.Attributes.Amount != paymentRequest.Amount  || input.Data.Attributes.Currency != paymentRequest.CurrencyId ||
                 (paymentRequest.ExternalTransactionId != null && input.Data.Id != paymentRequest.ExternalTransactionId.ToString()))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongInputParameters);
                if (input.Data.Attributes.Status.ToLower() == "processed")
                {
                    if (paymentRequest.Type== (int)PaymentRequestTypes.Deposit)
                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                    else
                    {
                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved,
                       string.Empty, null, null, false, string.Empty, documentBl, notificationBl);
                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                    }
                }
                else if (input.Data.Attributes.Status.ToLower() == "expired" ||
                         input.Data.Attributes.Status.ToLower() == "process_failed" ||
                         input.Data.Attributes.Status.ToLower() == "terminated" ||
                         input.Data.Attributes.Status.ToLower() == "canceled" ||
                         input.Data.Attributes.Status.ToLower() == "refunded")
                {
                    if (paymentRequest.Type== (int)PaymentRequestTypes.Deposit)
                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Data.Attributes.Resolution, notificationBl);
                    else
                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Data.Attributes.Resolution,
                                                            null, null, false, string.Empty, documentBl, notificationBl);
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
                Program.DbLogger.Error(ex);
            }
            return Ok(response);
        }

        private static string ComputeSha1(string rawData)
        {
            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] bytes = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}