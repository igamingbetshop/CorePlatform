using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.PaymentGateway.Models.Mpesa;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using System.ServiceModel;
using System;
using System.Net;
using System.Text;
using IqSoft.CP.PaymentGateway.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.CacheModels;
using System.Linq;
using IqSoft.CP.Common.Models;
using System.IO;
using System.Web;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class MpesaController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Mpesa);
        public static List<string> MpesaB2CWhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.MpesaB2C);
        public static List<string> MpesaC2BWhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.MpesaC2B);
        public static List<string> PayBillWhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.MpesaPayBill);
        
        [HttpPost]
        [Route("api/Mpesa/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput paymentInput)
        {
            var response = "Failed";
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream); // for log
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);
                var input = paymentInput.Body.StkCallback;
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Mpesa);
                            var paymentRequest = paymentSystemBl.GetPaymentRequestByExternalId(input.MerchantRequestID, paymentSystem.Id) ??
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                               client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            paymentRequest.ExternalTransactionId = input.CheckoutRequestID;
                            var amountData = input.CallbackMetadata.Item.FirstOrDefault(x => x.Name == "Amount");
                            if (amountData == null || !decimal.TryParse(amountData.Value, out decimal amount))
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestInValidAmount);
                            paymentRequest.Amount = amount;
                            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");
                            paymentInfo.WalletNumber = input.CallbackMetadata.Item.FirstOrDefault(x => x.Name == "MpesaReceiptNumber")?.Value;
                            paymentRequest.Info = JsonConvert.SerializeObject(paymentInfo);
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            if (input.ResultCode == 0)
                            {
                                clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds, comment: input.ResultDesc);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            }
                            else if (input.ResultCode != 0) // check codes
                                clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.ResultDesc, notificationBl);

                        }
                    }
                }
                response = "ok";
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "ok";
                }
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(paymentInput) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(paymentInput) + "_Error: " + ex.Message);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }

        [HttpPost]
        [Route("api/MpesaB2C/PaymentRequest")]
        public HttpResponseMessage PaymentRequest(DirectPaymentInput paymentInput)
        {
            var response = "Failed";
            var userIds = new List<int>();
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream); // for log
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                using (var documentBl = new DocumentBll(paymentSystemBl))
                {
                    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.MpesaB2C);
                    var paymentRequest = paymentSystemBl.GetPaymentRequestByExternalId(paymentInput.Result.ConversationID, paymentSystem.Id) ??
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                       client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    if (paymentInput.Result.ResultCode != 0)
                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, paymentInput.Result.ResultDesc,
                                                            null, null, false, paymentRequest.Parameters, documentBl, notificationBl, out userIds, true);
                    else
                    {
                        paymentRequest.ExternalTransactionId = paymentInput.Result.TransactionID;
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved,
                                   paymentInput.Result.ResultDesc, null, null, false, paymentRequest.Parameters, documentBl, notificationBl, out userIds);
                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                    }
                }
                response = "ok";
                foreach (var uId in userIds)
                {
                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "ok";
                }
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(paymentInput) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(paymentInput) + "_Error: " + ex.Message);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }

        [HttpPost]
        [Route("api/Mpesa/PayoutRequest")]
        public HttpResponseMessage PayoutRequest(PayoutInput input)
        {
            var response = "Failed";
            var userIds = new List<int>();
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream); // for log
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);
                BaseBll.CheckIp(WhitelistedIps, WebApiApplication.DbLogger);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                using (var documentBl = new DocumentBll(paymentSystemBl))
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TransactionReference)) ??
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                       client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    paymentRequest.ExternalTransactionId = input.MerchantId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    if (input.Amount != paymentRequest.Amount)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestInValidAmount);
                    if (input.TransactionStatus.ToUpper() == "SUCCESS")
                    {
                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, input.TransactionMessage,
                                                                       null, null, false, paymentRequest.Parameters, documentBl, notificationBl, out userIds);
                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                    }
                    else if (input.TransactionStatus.ToUpper() == "FAILED")
                    {
                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.TransactionMessage, null, null,
                                                            false, paymentRequest.Parameters, documentBl, notificationBl, out userIds);
                    }
                    foreach (var uId in userIds)
                    {
                        PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                    }
                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                    BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                    response = "ok";
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "ok";
                }
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Message);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }

        [HttpPost]
        [Route("api/C2B/{partnerId}/ValidationRequest")]
        public HttpResponseMessage ValidationRequest(int partnerId, PayBillInput paymentInput)
        {
            var paymentOutput = new DirectPaymentOutput { ResultCode = "0", ResultDesc = "Accepted" };
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream); // for log
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);
                BaseBll.CheckIp(MpesaC2BWhitelistedIps, WebApiApplication.DbLogger);
                var client = CacheManager.GetClientByMobileNumber(partnerId, paymentInput.MobileNumber) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if(client.CurrencyId != Constants.Currencies.KenyanShilling)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.MpesaC2B);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting.State != (int)PartnerPaymentSettingStates.Active &&
                   partnerPaymentSetting.State != (int)PartnerPaymentSettingStates.Hidden)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                if (partnerPaymentSetting.UserName != paymentInput.BusinessShortCode)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                if (paymentInput.TransAmount > partnerPaymentSetting.MaxAmount || paymentInput.TransAmount < partnerPaymentSetting.MinAmount)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestInValidAmount);
                if(client.State == (int)ClientStates.FullBlocked ||
                    client.State == (int)ClientStates.Suspended ||
                    client.State != (int)ClientStates.BlockedForDeposit ||
                    client.State != (int)ClientStates.Disabled )
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientBlocked);

            }
            catch (FaultException<BllFnErrorType> ex)
            {
                paymentOutput.ResultCode = MpesaHelpers.GetErrorCode(ex.Detail.Id);
                paymentOutput.ResultDesc = "Rejected";
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(paymentInput) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                paymentOutput.ResultCode = MpesaHelpers.GetErrorCode(Constants.Errors.GeneralException);
                paymentOutput.ResultDesc = "Rejected";
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(paymentInput) + "_Error: " + ex.Message);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(paymentOutput), Encoding.UTF8) };
        }

        [HttpPost]
        [Route("api/C2B/{partnerId}/PayBillRequest")]
        public HttpResponseMessage PayBillRequest(int partnerId, PayBillInput paymentInput)
        {
            var paymentOutput = new DirectPaymentOutput { ResultCode = "0", ResultDesc = "Accepted" };
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream); // for log
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);
                BaseBll.CheckIp(MpesaC2BWhitelistedIps, WebApiApplication.DbLogger);
                var client = CacheManager.GetClientByMobileNumber(partnerId, paymentInput.MobileNumber) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (client.CurrencyId != Constants.Currencies.KenyanShilling)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.MpesaC2B);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting.State != (int)PartnerPaymentSettingStates.Active &&
                   partnerPaymentSetting.State != (int)PartnerPaymentSettingStates.Hidden)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                if (partnerPaymentSetting.UserName != paymentInput.BusinessShortCode)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                if (paymentInput.TransAmount > partnerPaymentSetting.MaxAmount || paymentInput.TransAmount < partnerPaymentSetting.MinAmount)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestInValidAmount);

                var paymentRequest = new PaymentRequest
                {
                    Type = (int)PaymentRequestTypes.Deposit,
                    Amount = paymentInput.TransAmount,
                    ClientId = client.Id,
                    CurrencyId = client.CurrencyId,
                    PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                    PartnerPaymentSettingId = partnerPaymentSetting.Id,
                    ExternalTransactionId = paymentInput.TransID
                };
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                using (var scope = CommonFunctions.CreateTransactionScope())
                {
                    var request = clientBl.CreateDepositFromPaymentSystem(paymentRequest, out LimitInfo info, false);
                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds, comment: paymentInput.TransactionType);
                    foreach (var uId in userIds)
                    {
                        PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                    }
                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                    BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                    BaseHelpers.BroadcastDepositLimit(info);
                    scope.Complete();
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                paymentOutput.ResultCode = MpesaHelpers.GetErrorCode(ex.Detail.Id);
                paymentOutput.ResultDesc = "Rejected";
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(paymentInput) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                paymentOutput.ResultCode = MpesaHelpers.GetErrorCode(Constants.Errors.GeneralException);
                paymentOutput.ResultDesc = "Rejected";
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(paymentInput) + "_Error: " + ex.Message);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(paymentOutput), Encoding.UTF8) };
        }

    }
}