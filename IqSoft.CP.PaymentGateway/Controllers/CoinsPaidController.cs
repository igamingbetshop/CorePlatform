using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.CoinsPaid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{

    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class CoinsPaidController : ApiController
    {

        [Route("api/CoinsPaid/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var response = string.Empty;
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info(inputString);
                var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
                var isDeposit = input.Type == "deposit_exchange";
                if (input.Status == "confirmed" || input.Status == "not_confirmed")
                {
                    if (input.Type == "deposit_exchange")
                        PaymentRequest(input, inputString);
                    else if (input.Type == "withdrawal_exchange")
                        PayoutRequest(input, inputString);
                }
                else if(input.Status == "cancelled")
                {
                    if (!string.IsNullOrEmpty(input.Transactions?.FirstOrDefault()?.TxId))
                    {
                        using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                        {
                            using (var clientBl = new ClientBll(paymentSystemBl))
                            {
                                using (var notificationBl = new NotificationBll(clientBl))
                                {
                                    using (var documentBl = new DocumentBll(paymentSystemBl))
                                    {
                                        var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.CoinsPaid + input.CryptoAddress.Currency.ToUpper());
                                        if (paymentSystem == null)
                                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                                        var paymentRequest = paymentSystemBl.GetPaymentRequestByExternalId(input.Transactions.FirstOrDefault().Id.ToString(), paymentSystem.Id);
                                        if (input.Transactions.FirstOrDefault().Type == "deposit")
                                        {
                                            clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Error, notificationBl);
                                        }
                                        else if (input.Transactions.FirstOrDefault().Type == "withdrawal")
                                        {
                                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Error, null,
                                                                           null, false, paymentRequest.Parameters, documentBl, notificationBl, out List<int> userIds);
                                            foreach (var uId in userIds)
                                            {
                                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
				}
                response = "OK";
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null)
                {
                    if (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists || ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)
                        response = "OK";
                    else
                        response = ex.Detail.Message;
                }
                else
                    response = ex.Message;
                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(response);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(httpResponseMessage));
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
        }
        //public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
        //{
        //    var response = string.Empty;
        //    try
        //    {
        //        var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
        //        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(inputString));
        //        using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
        //        {
        //            using (var clientBl = new ClientBll(paymentSystemBl))
        //            {
        //                var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
        //                var isDeposit = input.Type == "deposit_exchange";
        //                var prId = isDeposit ? input.CryptoAddress.ForeignId : input.ForeignId;
        //                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(prId));
        //                if (paymentRequest == null)
        //                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
        //                var signature = HttpContext.Current.Request.Headers.Get("X-Processing-Signature");
        //                var client = CacheManager.GetClientById(paymentRequest.ClientId);
        //                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.CoinsPaid + input.CryptoAddress.Currency.ToUpper());
        //                if (paymentSystem == null)
        //                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
        //                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
        //                if (partnerPaymentSetting == null)
        //                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
        //                var sign = CommonFunctions.ComputeHMACSha512(inputString, partnerPaymentSetting.Password).ToLower();
        //                if (sign != signature)
        //                {
        //                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        //                }
        //                if (isDeposit)
        //                    paymentRequest.Amount = Math.Round(Convert.ToDecimal(input.CurrencyReceived.AmountMinusFee));
        //                paymentRequest.ExternalTransactionId = input.Transactions.FirstOrDefault().Txid;
        //                paymentRequest.Info = JsonConvert.SerializeObject(input);
        //                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
        //                if (input.Status == "confirmed")
        //                {
        //                    if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
        //                    {
        //                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
        //                    }
        //                    else
        //                    {
        //                        using (var documentBll = new DocumentBll(paymentSystemBl))
        //                        {
        //                            using (var notificationBl = new NotificationBll(clientBl))
        //                            {
        //                                var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
        //                                 null, null, false, paymentRequest.Parameters, documentBll);
        //                                clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
        //                            }
        //                        }
        //                    }
        //                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
        //                    BaseHelpers.BroadcastBalance(paymentRequest.ClientId);
        //                }
        //            }
        //        }
        //    }
        //    catch (FaultException<BllFnErrorType> ex)
        //    {
        //        if (ex.Detail != null)
        //        {
        //            if (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists || ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)
        //                response = "OK";
        //            else if (ex.Detail != null && ex.Detail.Id == Constants.Errors.WrongHash)
        //            {
        //                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
        //            }
        //        }
        //        WebApiApplication.DbLogger.Error(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        response = ex.Message;
        //        WebApiApplication.DbLogger.Error(response);
        //    }
        //    var a = new HttpResponseMessage
        //    {
        //        StatusCode = HttpStatusCode.OK,
        //        Content = new StringContent(response, Encoding.UTF8)
        //    };
        //    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(a));
        //    return new HttpResponseMessage
        //    {
        //        StatusCode = HttpStatusCode.OK,
        //        Content = new StringContent(response, Encoding.UTF8)
        //    };
        //}

        private void PaymentRequest(PaymentInput input, string inputString)
        {
            var client = CacheManager.GetClientById(Convert.ToInt32(input.CryptoAddress.ForeignId));
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.CoinsPaid + input.CryptoAddress.Currency.ToUpper());
            if (paymentSystem == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
            var allowMultiCurrencyAccounts = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.AllowMultiCurrencyAccounts);
            var amca = allowMultiCurrencyAccounts != null && allowMultiCurrencyAccounts.Id > 0 && allowMultiCurrencyAccounts.NumericValue == 1;

            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id,
                                                                               amca ? input.CurrencyReceived.Currency : client.CurrencyId, 
                                                                               (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
            if (partnerPaymentSetting.State != (int)PartnerPaymentSettingStates.Active)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingBlocked);
            if (client.State == (int)ClientStates.BlockedForDeposit || client.State == (int)ClientStates.FullBlocked ||
                client.State == (int)ClientStates.Suspended || client.State == (int)ClientStates.SuspendedWithWithdraw ||  
                client.State == (int)ClientStates.Disabled)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientBlocked);
            if (Convert.ToDecimal(input.CurrencyReceived.Amount) < 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

            var amount = Math.Round(Convert.ToDecimal(input.CurrencyReceived.Amount));
            var parameters = new Dictionary<string, string>();
            var currencyId = input.CurrencyReceived.Currency;
            
            if (!amca && client.CurrencyId != input.CurrencyReceived.Currency)
            {
                var rate = BaseBll.GetCurrenciesDifference(input.CurrencyReceived.Currency, client.CurrencyId);
                parameters.Add("Currency", input.CurrencyReceived.Currency);
                parameters.Add("AppliedRate", rate.ToString("F"));
                amount = Math.Round(rate * amount);
                currencyId = client.CurrencyId;
            }
            var paymentRequest = new PaymentRequest
            {
                Amount = amount,
                ClientId = client.Id,
                CurrencyId = currencyId,
                PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                PartnerPaymentSettingId = partnerPaymentSetting.Id,
                ExternalTransactionId = input.Transactions?.FirstOrDefault()?.Id.ToString(),
                Info = inputString,
                Parameters = JsonConvert.SerializeObject(parameters)
            };
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var scope = CommonFunctions.CreateTransactionScope())
                    {
                        var request = paymentSystemBl.GetPaymentRequestByExternalId(input.Transactions?.FirstOrDefault()?.TxId, paymentSystem.Id);
                        if (request == null)
                        {
                            request = clientBl.CreateDepositFromPaymentSystem(paymentRequest, out LimitInfo info, false);
                            PaymentHelpers.InvokeMessage("PaymentRequst", request.ClientId);
                            if (input.Status == "confirmed")
                            {
                                request.ExternalTransactionId = input.Transactions?.FirstOrDefault()?.TxId;
                                request.Amount = amount;
                                request.Parameters = paymentRequest.Parameters;
                                paymentSystemBl.ChangePaymentRequestDetails(request);

                                if (request.Amount < partnerPaymentSetting.MinAmount || request.Amount > partnerPaymentSetting.MaxAmount)
                                {
                                    scope.Complete();
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestInValidAmount);
                                }
                                clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                                BaseHelpers.BroadcastDepositLimit(info);
                            }
                        }
                        scope.Complete();
                    }
                }
            }
        }

        private void PayoutRequest(PaymentInput input, string inputString)
        {
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.ForeignId));
                    if (paymentRequest == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var signature = HttpContext.Current.Request.Headers.Get("X-Processing-Signature");
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    if (partnerPaymentSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                    var sign = CommonFunctions.ComputeHMACSha512(inputString, partnerPaymentSetting.Password).ToLower();
                    if (sign != signature)
                    {
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    }
                    paymentRequest.ExternalTransactionId = input.Transactions.FirstOrDefault().TxId;
                    paymentRequest.Info = JsonConvert.SerializeObject(input);
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    using (var documentBll = new DocumentBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                             null, null, false, paymentRequest.Parameters, documentBll, notificationBl, out List<int> userIds);
                            foreach (var uId in userIds)
                            {
                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                            }
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                        }
                    }
                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                    BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                }
            }
        }
    }
}