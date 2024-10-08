using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using System.IO;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using IqSoft.CP.PaymentGateway.Models.Praxis;
using System;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models;
using System.Collections.Generic;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.PaymentGateway.Helpers;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class PraxisController : ApiController
    {
        private static readonly List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Praxis);
        [HttpPost]
        [Route("api/Praxis/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = new PaymentOutput { Status = 0, Description = "Ok" };
            BllPartnerPaymentSetting partnerPaymentSetting = new BllPartnerPaymentSetting();
            try
            {
                //  BaseBll.CheckIp(WhitelistedIps);
                var inputSign = HttpContext.Current.Request.Headers.Get("GT-Authentication");
                if (string.IsNullOrEmpty(inputSign))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger, 60))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Session.OrderId)) ??
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(request.ClientId.Value);
                            partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                           client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
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
                            if (request.Amount > input.Transaction.Amount / 100)
                            {
                                if (parameters.ContainsKey("InitialAmount"))
                                    parameters["InitialAmount"] = request.Amount.ToString("F");
                                else
                                    parameters.Add("InitialAmount", request.Amount.ToString("F"));
                                if (parameters.ContainsKey("UpdatedAmount"))
                                    parameters["UpdatedAmount"] = (input.Transaction.Amount /100).ToString("F");
                                else
                                    parameters.Add("UpdatedAmount", (input.Transaction.Amount /100).ToString("F"));
                                request.Amount = input.Transaction.Amount /100;
                                request.Parameters = JsonConvert.SerializeObject(parameters);
                            }
                            request.ExternalTransactionId = input.Transaction.Tid.ToString();
                            paymentSystemBl.ChangePaymentRequestDetails(request);
                            var transactionStatus = input.Transaction.Status.ToLower();
                            if (transactionStatus == "approved")
                            {
                                clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                            else if (transactionStatus == "rejected" || transactionStatus == "cancelled" ||
                                     transactionStatus == "error" || transactionStatus == "chargeback")
                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.Transaction.Status, notificationBl);
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && ex.Detail.Id != Constants.Errors.ClientDocumentAlreadyExists &&
                    ex.Detail.Id != Constants.Errors.RequestAlreadyPayed)
                {
                    response.Status = 1;
                    response.Description = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                }
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {bodyStream.ReadToEnd()}" +
                                                 $" Response: {JsonConvert.SerializeObject(response)}");
            }
            catch (Exception ex)
            {
                response.Status = -1;
                response.Description = ex.Message;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
            }
            response.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            HttpContext.Current.Response.Headers.Add("GT-Authentication",
                CommonFunctions.ComputeSha384(response.Status.ToString() + response.Timestamp.ToString() + partnerPaymentSetting.Password).ToLower());
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
        }

        [HttpPost]
        [Route("api/Praxis/Authentication")]
        public HttpResponseMessage Authentication(PaymentInput input)
        {
            var response = new PaymentOutput { Status = 0, Description = "Ok" };
            BllPartnerPaymentSetting partnerPaymentSetting = new BllPartnerPaymentSetting();
            var userIds = new List<int>();
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);  // FOR LOG
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info(inputString);

                //  BaseBll.CheckIp(WhitelistedIps);
                var inputSign = HttpContext.Current.Request.Headers.Get("GT-Authentication");
                if (string.IsNullOrEmpty(inputSign))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBl = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(documentBl))
                            {
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Session.OrderId));
                                if (request == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
                                partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                               client.CurrencyId, request.Type);

                                var signature = CommonFunctions.ComputeSha384(partnerPaymentSetting.UserName + parameters["ApplicationKey"] +
                                                                  input.Timestamp + input.Customer.Token + input.Session.OrderId +
                                                                  input.Transaction.Tid + input.Transaction.Currency + input.Transaction.Amount +
                                                                  (input.Transaction.ConversionRate ?? string.Empty) +
                                                                  (input.Transaction?.ProcessedCurrency ?? string.Empty) +
                                                                  (input.Transaction.ProcessedAmount ?? string.Empty) +
                                                                  partnerPaymentSetting.Password).ToLower();

                                if (inputSign.ToLower() != signature.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                                var paymentSystem = CacheManager.GetPaymentSystemById(request.PaymentSystemId);
                                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(request.Info);
                                /*if (paymentSystem.Name == Constants.PaymentSystems.PraxisCard)
                                {
                                    paymentInfo.CardNumber = input.Transaction.CardDetails.CardNumber;
                                    paymentInfo.AccountType = input.Transaction.CardDetails.Type;
                                    paymentInfo.ExpiryDate = input.Transaction.CardDetails.ExpDate;
                                    paymentInfo.BankName = input.Transaction.CardDetails.BankName;
                                    paymentInfo.TrackingNumber = input.Transaction.CardDetails.Token;
                                    request.CardNumber = paymentInfo.CardNumber;
                                }
                                else*/
                                {
                                    paymentInfo.WalletNumber = input.Transaction.WalletDetails?.AccountIdentifier;
                                    paymentInfo.TrackingNumber = input.Transaction.WalletDetails?.Token;
                                }
                                paymentInfo.Provider = input.Session.PaymentMethod;
                                paymentInfo.PSPService = input.Session.Gateway;
                                request.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    DefaultValueHandling = DefaultValueHandling.Ignore
                                });
                                request.ExternalTransactionId = input.Transaction.Tid.ToString();
                                paymentSystemBl.ChangePaymentRequestDetails(request);
                                var transactionStatus = input.Transaction.Status.ToLower();
                                if (transactionStatus == "approved" && request.Type == (int)PaymentRequestTypes.Withdraw &&
                                  (request.Status == (int)PaymentRequestStates.Pending || request.Status == (int)PaymentRequestStates.PayPanding))
                                {
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, transactionStatus,
                                                                                   null, null, false, request.Parameters, documentBl, notificationBl, out userIds, changeFromPaymentSystem: true);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                }
                                else if (transactionStatus == "rejected" || transactionStatus == "cancelled" ||
                                    transactionStatus == "error" || transactionStatus == "chargeback")
                                {
                                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, input.Transaction.Status,
                                                                        null, null, false, request.Parameters, documentBl, notificationBl, out userIds);
                                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                }

                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && ex.Detail.Id != Constants.Errors.ClientDocumentAlreadyExists &&
                    ex.Detail.Id != Constants.Errors.RequestAlreadyPayed)
                {
                    response.Status = 1;
                    response.Description = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                }
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {bodyStream.ReadToEnd()}" +
                                                 $" Response: {JsonConvert.SerializeObject(response)}");
            }
            catch (Exception ex)
            {
                response.Status = -1;
                response.Description = ex.Message;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {JsonConvert.SerializeObject(response)}");
            }
            response.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            HttpContext.Current.Response.Headers.Add("GT-Authentication",
                CommonFunctions.ComputeSha384(response.Status.ToString() + response.Timestamp.ToString() + partnerPaymentSetting.Password).ToLower());

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
        }
    }
}