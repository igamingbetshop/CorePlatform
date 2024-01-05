using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using System;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.PaymentGateway.Models.Praxis;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models;
using System.Collections.Generic;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class PraxisController : ControllerBase
    {
        [HttpPost]
        [Route("api/Praxis/ApiRequest")]
        public ActionResult ApiRequest(PaymentInput input)
        {
            var response = new PaymentOutput { Status = 0, Description = "Ok" };
            BllPartnerPaymentSetting partnerPaymentSetting = new BllPartnerPaymentSetting();
            try
            {
                //  BaseBll.CheckIp(WhitelistedIps);
                var inputSign = string.Empty;
                if (Request.Headers.TryGetValue("GT-Authentication", out StringValues header))
                    inputSign = header.ToString();
                Program.DbLogger.Info("GT-Authentication: " + inputSign);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Session.OrderId));
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
                var merchant = partnerPaymentSetting.UserName.Split(',');
                var signature = CommonFunctions.ComputeSha384(merchant[0] + parameters["ApplicationKey"] +
                                                  input.Timestamp + input.Customer.Token + input.Session.OrderId +
                                                  input.Transaction.Tid + input.Transaction.Currency + input.Transaction.Amount +
                                                  (input.Transaction.ConversionRate ?? string.Empty) +
                                                  (input.Transaction?.ProcessedCurrency ?? string.Empty)  +
                                                  (input.Transaction.ProcessedAmount ?? string.Empty)  +
                                                  partnerPaymentSetting.Password).ToLower();

                if (inputSign.ToLower() != signature.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                request.ExternalTransactionId = input.Transaction.Tid.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(request);
                var transactionStatus = input.Transaction.Status.ToLower();
                if (transactionStatus== "approved")
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                else if (transactionStatus == "rejected" || transactionStatus == "cancelled" ||
                    transactionStatus == "error" || transactionStatus == "chargeback")
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.Transaction.Status, notificationBl);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && ex.Detail.Id != Constants.Errors.ClientDocumentAlreadyExists &&
                    ex.Detail.Id != Constants.Errors.RequestAlreadyPayed)
                {
                    response.Status = 1;
                    response.Description = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                }
                Program.DbLogger.Error(ex.Detail);
            }
            catch (Exception ex)
            {
                response.Status = -1;
                response.Description = ex.Message;
                Program.DbLogger.Error(ex);
            }
            response.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Response.Headers.Add("GT-Authentication",
                CommonFunctions.ComputeSha384(response.Status.ToString() + response.Timestamp.ToString() + partnerPaymentSetting.Password).ToLower());
            return Ok(response);
        }

        [HttpPost]
        [Route("api/Praxis/Authentication")]
        public ActionResult Authentication(PaymentInput input)
        {
            var response = new PaymentOutput { Status = 0, Description = "Ok" };
            BllPartnerPaymentSetting partnerPaymentSetting = new BllPartnerPaymentSetting();
            try
            {
                var inputSign = string.Empty;
                if (Request.Headers.TryGetValue("GT-Authentication", out StringValues header))
                    inputSign = header.ToString();
                Program.DbLogger.Info("GT-Authentication: " + inputSign);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var documentBl = new DocumentBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Session.OrderId));
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
                partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                  client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var merchant = partnerPaymentSetting.UserName.Split(',');
                var signature = CommonFunctions.ComputeSha384(merchant[0] + parameters["ApplicationKey"] +
                                                  input.Timestamp + input.Customer.Token + input.Session.OrderId +
                                                  input.Transaction.Tid + input.Transaction.Currency + input.Transaction.Amount +
                                                  (input.Transaction.ConversionRate ?? string.Empty) +
                                                  (input.Transaction?.ProcessedCurrency ?? string.Empty) +
                                                  (input.Transaction.ProcessedAmount ?? string.Empty) +
                                                  partnerPaymentSetting.Password).ToLower();

                if (inputSign.ToLower() != signature.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var paymentSystem = CacheManager.GetPaymentSystemById(request.PaymentSystemId);
                if (paymentSystem.Name == Constants.PaymentSystems.PraxisCard)
                {
                    var paymentInfo = new PaymentInfo
                    {
                        CardNumber = input.Transaction.CardDetails.CardNumber,
                        AccountType = input.Transaction.CardDetails.Type,
                        ExpiryDate = input.Transaction.CardDetails.ExpDate,
                        BankName = input.Transaction.CardDetails.BankName,
                        TrackingNumber = input.Transaction.CardDetails.Token
                    };
                    request.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                    request.CardNumber = paymentInfo.CardNumber;
                }
                else
                {
                    var paymentInfo = new PaymentInfo
                    {
                        WalletNumber = input.Transaction.WalletDetails.AccountIdentifier,
                        TrackingNumber = input.Transaction.WalletDetails.Token
                    };
                    request.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                }
                request.ExternalTransactionId = input.Transaction.Tid.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(request);
                var transactionStatus = input.Transaction.Status.ToLower();
                if (transactionStatus == "rejected" || transactionStatus == "cancelled" ||
                    transactionStatus == "error" || transactionStatus == "chargeback")
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.Transaction.Status, notificationBl);
                clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed,
                                                    input.Transaction.Status, null, null, false, request.Parameters, documentBl, notificationBl);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && ex.Detail.Id != Constants.Errors.ClientDocumentAlreadyExists &&
                    ex.Detail.Id != Constants.Errors.RequestAlreadyPayed)
                {
                    response.Status = 1;
                    response.Description = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                }
                Program.DbLogger.Error(ex.Detail);
            }
            catch (Exception ex)
            {
                response.Status = -1;
                response.Description = ex.Message;
                Program.DbLogger.Error(ex);
            }
            response.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Response.Headers.Add("GT-Authentication",
                CommonFunctions.ComputeSha384(response.Status.ToString() + response.Timestamp.ToString() + partnerPaymentSetting.Password).ToLower());

            return Ok(response);
        }
    }
}