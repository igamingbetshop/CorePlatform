using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Instapay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
    public class InstapayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Instapay);
        [HttpPost]
        [Route("api/Instapay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var status = "success";
            var userIds = new List<int>();
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TransactionId));
                                if (paymentRequest == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);

                                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
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
                                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds);
                                    }
                                    else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                                    {
                                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                          null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    }
                                }
                                else if (input.Status.ToLower() == "rejected")
                                {
                                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                    if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.ErrorMessage, notificationBl);
                                    else
                                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, 
                                            input.ErrorMessage, null, null, false, string.Empty, documentBll, notificationBl, out userIds);

                                }
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (!(ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)))
                {
                    var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                    WebApiApplication.DbLogger.Error(exp);
                    status = exp.Message;
                }
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new { status }), Encoding.UTF8)
            };
        }

        [HttpPost]
        [Route("api/Cepbank/ApiRequest")]
        public HttpResponseMessage CepbankRequest(CepbankInput input)
        {
            var status = "success";
            var userIds = new List<int>();
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
              //  BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.PaymentRequestId));
                                if (paymentRequest == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                                   client.CurrencyId, paymentRequest.Type);

                                var hash = string.Format("{0}{1}{2}{3}{4}{5}{6}", input.TransactionType, paymentRequest.Id, client.Id,
                                            input.Status, input.Amount, input.DateTime, partnerPaymentSetting.Password);
                                if (CommonFunctions.ComputeMd5(hash).ToLower() != input.Hash.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                                paymentRequest.ExternalTransactionId = input.InstTransactionId;
                                if (input.Status.ToLower() == "approved")
                                {
                                    if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                                    {
                                        if (client.CurrencyId != Constants.Currencies.TurkishLira)
                                        {
                                            var rate = BaseBll.GetPaymentCurrenciesDifference(Constants.Currencies.TurkishLira, client.CurrencyId, partnerPaymentSetting);
                                            input.Amount = Math.Round(rate * input.Amount, 2);
                                            var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                                                             JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                                            parameters.Add("Currency", Constants.Currencies.TurkishLira );
                                            parameters.Add("AppliedRate", rate.ToString("F"));
                                            paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                                        }
                                        paymentRequest.Amount = input.Amount;
                                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds);
                                    }
                                    else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                                    {
                                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                          null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    }
                                }
                                else if (input.Status.ToLower() == "rejected")
                                {
                                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                    if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Note, notificationBl);
                                    else
                                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Note,
                                                                            null, null, false, paymentRequest.Parameters, documentBll, notificationBl, out userIds);

                                }
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (!(ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)))
                {
                    var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                    WebApiApplication.DbLogger.Error(exp);
                    status = exp.Message;
                }
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new { status }), Encoding.UTF8)
            };
        }


        [HttpPost]
        [Route("api/ExpressHavale/ApiRequest")]
        public HttpResponseMessage ExpressHavaleRequest(ExpressHavaleInput input)
        {
            var status = "success";
            var userIds = new List<int>();
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);
                //  BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {                               
                                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.PaymentRequestId));
                                if (paymentRequest == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                                   client.CurrencyId, paymentRequest.Type);

                                var hash = string.Format("{0}{1}{2}",  paymentRequest.Id, client.Id, partnerPaymentSetting.Password);                     
                                if (CommonFunctions.ComputeMd5(hash).ToLower() != input.Hash.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                                paymentRequest.ExternalTransactionId = input.InstTransactionId;
                                if (input.Status.ToLower() == "approved")
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
                                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds);
                                    }
                                    else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                                    {
                                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                          null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    }
                                }
                                else if (input.Status.ToLower() == "rejected")
                                {
                                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                    if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.ErrorMessage, notificationBl);
                                    else
                                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.ErrorMessage,
                                                                            null, null, false, paymentRequest.Parameters, documentBll, notificationBl, out userIds);

                                }
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (!(ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)))
                {
                    var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                    WebApiApplication.DbLogger.Error(exp);
                    status = exp.Message;
                }
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new { status }), Encoding.UTF8)
            };
        }

    }
}