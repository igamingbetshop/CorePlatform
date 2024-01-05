// Author: Varsik Harutyunyan
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.PaymentGateway.Models.NOWPay;
using IqSoft.CP.Integration.Payments.Models.NOWPay;
using IqSoft.CP.Common.Models.CacheModels;
using System.IO;
using System.Linq;
using static IqSoft.CP.Common.Constants;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class NOWPayController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "142.132.186.26",
            "144.76.201.30"
        };
        [HttpPost]
        [Route("api/NOWPay/ApiRequest")]
        public ActionResult ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    using (var notificationBl = new NotificationBll(paymentSystemBl))
                    {
                        var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Order_id));
                        if (request == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(request.ClientId);

                        var ip = string.Empty;
                        if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                            ip = header.ToString();
                        BaseBll.CheckIp(WhitelistedIps, ip);

                        request.ExternalTransactionId = input.Payment_id;
                        request.Info = JsonConvert.SerializeObject(input);
                        request.Amount = Math.Round(input.Price_amount * input.Actually_paid / input.Pay_amount, 2);

                        paymentSystemBl.ChangePaymentRequestDetails(request);

                        if (input.Payment_status.ToUpper() == "FINISHED")
                            clientBl.ApproveDepositFromPaymentSystem(request, false);
                        else if (input.Payment_status.ToUpper() == "FAILED" || input.Payment_status.ToUpper() == "FAILED" || input.Payment_status.ToUpper() == "EXPIRED")
                        {
                            clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.Order_description, notificationBl);
                        }
                    }
                }
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
                return Conflict(new StringContent(ex.Message, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                response = ex.Message;
                Program.DbLogger.Error(response);
                return Conflict(new StringContent(ex.Message, Encoding.UTF8));
            }

            return Ok(new StringContent("OK", Encoding.UTF8));
        }

        [HttpPost]
        [Route("api/NOWPay/PayoutRequest")]
        public ActionResult PayoutRequest(PayoutOutput input)
        {
            var response = "OK";
            try
            {
                var bodyStream = new StreamReader(Request.Body);
                var inputString = bodyStream.ReadToEnd();

                Program.DbLogger.Info(inputString);
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(clientBl))
                            {
                                var transaction = JsonConvert.DeserializeObject<Withdrawal>(inputString);
                                if (transaction.Status.ToLower() == "finished")
                                {
                                    var request = paymentSystemBl.GetPaymentRequestByExternalId(transaction.Batch_withdrawal_id, CacheManager.GetPaymentSystemByName(PaymentSystems.NOWPay).Id);
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                                    null, null, false, string.Empty, documentBll, notificationBl);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                }
                                else if (transaction.Status.ToLower() == "failed" || transaction.Status.ToLower() == "rejected")
                                {
                                    var request = paymentSystemBl.GetPaymentRequestByExternalId(transaction.Batch_withdrawal_id, CacheManager.GetPaymentSystemByName(PaymentSystems.NOWPay).Id);
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, string.Empty,
                                                    null, null, false, string.Empty, documentBll, notificationBl);
                                }
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                    return Ok(new StringContent("OK", Encoding.UTF8));
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);
                response = exp.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = ex.Message;
            }
            return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8));
        }
    }
}
