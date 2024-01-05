using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Interkassa;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class InterkassaController : ControllerBase
    {
        [HttpPost]
        [Route("api/Interkassa/ApiRequest")]
        public ActionResult ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var response = string.Empty;
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                Program.DbLogger.Info(JsonConvert.SerializeObject(inputString));
                var dict = HttpUtility.ParseQueryString(inputString);
                var x = dict.AllKeys.ToDictionary(k => k, k => dict[k]);
                var aaa = JsonConvert.SerializeObject(x);
                var input = JsonConvert.DeserializeObject<PaymentInput>(aaa);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.PaymentNumber));
                if (paymentRequest == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Interkassa);
                if (paymentSystem == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var orderdParams = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}:{10}:{11}:{12}:{13}:{14}:{15}:{16}:",
                                                          input.Amount, input.CheckoutId, input.CheckoutPurseId, input.CheckoutRefund,
                                                          input.Currency, input.CardNumber, input.Description, input.InvoiceCreated,
                                                          input.InvoiceId, input.InvoiceProcessed, input.InvoiceState, input.PaymentCurrency,
                                                          input.PaymentMethod, input.PaymentNumber, input.PaysystemPrice, input.PaywayVia,
                                                          input.TransactionId);
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(orderdParams + "wU5vsPd1xR3QVSPdAgORnGjkCUgg2jVs"));
                    var sign = Convert.ToBase64String(bytes);
                    if (sign != input.Signature)
                    {
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    }
                }
                paymentRequest.Info = JsonConvert.SerializeObject(input);
                paymentRequest.ExternalTransactionId = input.InvoiceId;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                if (input.InvoiceState == "success")
                {
                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
                    //BaseHelpers.BroadcastBalance(paymentRequest.ClientId);
                }
                else
                {
                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, null, notificationBl);
                }
                return Ok("OK");
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                          ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "OK";
                }
                else
                {
                    response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                    httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
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