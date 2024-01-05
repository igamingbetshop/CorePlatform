using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Instapay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class InstapayController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "3.126.235.11",
            "67.215.1.66",
            "67.215.1.42",
            "173.209.48.130"
        };

        [HttpPost]
        [Route("api/Instapay/ApiRequest")]
        public ActionResult ApiRequest(PaymentInput input)
        {
            var status = "success";
            try
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var documentBl = new DocumentBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TransactionId));
                if (paymentRequest == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);

                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   client.CurrencyId, paymentRequest.Type);
                var hash = paymentSystem.Name == Constants.PaymentSystems.InstaMFT || paymentSystem.Name == Constants.PaymentSystems.InstaKK ?
                    string.Format("{0}{1}{2}", paymentRequest.Id, client.Id, partnerPaymentSetting.UserName) :
                    string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", input.RequestType, input.TransactionType, paymentRequest.Id, client.Id,
                                  input.Status, input.Amount, input.DateTime, partnerPaymentSetting.UserName);
                if (CommonFunctions.ComputeMd5(hash).ToLower() != input.Hash)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                paymentRequest.ExternalTransactionId = string.IsNullOrEmpty(input.InstTransactionId) ? input.TransactionId : input.InstTransactionId;
                if (input.Status.ToLower() == "approved" && (string.IsNullOrEmpty(input.RequestType) || input.RequestType.ToLower() == "normal"))
                {
                    if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                    {
                        if (client.CurrencyId != Constants.Currencies.TurkishLira)
                        {
                            var rate = BaseBll.GetPaymentCurrenciesDifference(Constants.Currencies.TurkishLira, client.CurrencyId, partnerPaymentSetting);
                            input.Amount = Math.Round(rate * input.Amount, 2);
                            var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                                             JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                            parameters.Add("Currency", Constants.Currencies.TurkishLira);
                            parameters.Add("AppliedRate", rate.ToString("F"));
                            paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                        }
                        paymentRequest.Amount = input.Amount;
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                    }
                    else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                    {
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                          null, null, false, string.Empty, documentBl, notificationBl);
                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                    }
                }
                else if (input.Status.ToLower() == "rejected")
                {
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.ErrorMessage, notificationBl);
                    else
                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.ErrorMessage, null, null, 
                                                            false, string.Empty, documentBl, notificationBl);
                }
                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (!(ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)))
                {
                    var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                    Program.DbLogger.Error(exp);
                    status = exp.Message;
                }
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                status = ex.Message;
            }
         
            return Ok(new StringContent(JsonConvert.SerializeObject(new { status }), Encoding.UTF8));
        }
    }
}