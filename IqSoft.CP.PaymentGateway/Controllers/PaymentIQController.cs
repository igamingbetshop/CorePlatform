using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.PaymentIQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class PaymentIQController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.PaymentIQ);

        [HttpPost]
        [Route("api/paymentiq/verifyuser")]
        public HttpResponseMessage VerifyUser(VerifyUserInput input)
        {
            var response = new VerifyOutput();
            var userIp = string.Empty;
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                var client = CacheManager.GetClientById(input.ClientId);
                if (client== null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var regionBl = new RegionBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.SessionId));
                    if (paymentRequest == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);

                    var clientSession = paymentSystemBl.GetClientSessionById(paymentRequest.SessionId ?? 0);
                    var region = regionBl.GetRegionByCountryCode(clientSession != null ? clientSession.Country : "SE");
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                    userIp = paymentInfo.TransactionIp;
                    response.UserId= client.Id.ToString();
                    response.UserCurrency = paymentRequest.CurrencyId;
                    response.Balance = Math.Round(paymentRequest.Amount, 2);
                    response.FirstName = client.FirstName;
                    response.LastName = client.LastName;
                    response.UserCat =  CacheManager.GetClientCategory(client.CategoryId).Name;
                    response.KycStatus = client.IsDocumentVerified ? "Approved" : "Pending";
                    response.Gender =  ((Gender)client.Gender).ToString().ToUpper();
                    response.Street = client.Address;
                    response.City = region.NickName;
                    response.State = region.IsoCode;
                    response.Country = region.IsoCode3;
                    response.Zip = string.IsNullOrEmpty(client.ZipCode?.Trim()) ? client.Id.ToString() : client.ZipCode.Trim();
                    response.Email = client.Email;
                    response.Dob = client.BirthDate.ToString("yyyy-MM-dd");
                    response.MobileNumber =  client.MobileNumber;
                    response.Locale = Integration.CommonHelpers.LanguageISOCodes[clientSession != null ? clientSession.LanguageId : "sw"];
                    response.Success = true;
                    response.Attributes = new AttributeModel { MerchantTransactionId = paymentRequest.Id };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Detail.Message;
                response.ErrorCode = ex.Detail.Id;
                WebApiApplication.DbLogger.Error(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                response.ErrorCode = Constants.Errors.GeneralException;
                WebApiApplication.DbLogger.Error(ex);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,                
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            httpResponseMessage.Content.Headers.Add("PIQ-Client-IP", userIp);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("api/paymentiq/authorize")]
        public HttpResponseMessage AuthorizeUser(AuthorizeInput input)
        {
            var response = new AuthorizeOutput();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(input.Attributes.MerchantTransactionId);
                    if (paymentRequest== null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                    if (client.Id.ToString() != input.UserId || paymentRequest.CurrencyId != input.Currency ||
                       (paymentRequest.Amount != Math.Abs(input.Amount) && paymentRequest.Type != (int)PaymentRequestTypes.Withdraw &&
                        paymentSystem.Name != Constants.PaymentSystems.PaymentIQCryptoPay && paymentSystem.Name != Constants.PaymentSystems.PaymentIQInterac))
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                    
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");
                    paymentInfo.MaskedAccount = input.MaskedAccount;
                    paymentInfo.BankAccountHolder = input.AccountHolder;
                    paymentInfo.TxTypeId = input.TxTypeId;
                    paymentInfo.TxName = input.TxName;
                    paymentInfo.Provider = input.Provider;
                    paymentInfo.PSPService = input.PSPService;
                    paymentRequest.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                    paymentRequest.ExternalTransactionId = input.TransactionId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    response.Success = true;
                    response.UserId = input.UserId;
                    response.MerchantTxId = paymentRequest.Id.ToString();
                    response.AuthCode = paymentRequest.Id.ToString();
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Detail.Message;
                response.ErrorCode = ex.Detail.Id;
                WebApiApplication.DbLogger.Error(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                response.ErrorCode = Constants.Errors.GeneralException;
                WebApiApplication.DbLogger.Error(ex);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("api/paymentiq/transfer")]
        public HttpResponseMessage TransferRequest(AuthorizeInput input)
        {
            var response = new AuthorizeOutput();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(input.Attributes.MerchantTransactionId);
                            if (paymentRequest== null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            if ((paymentSystem.Name == Constants.PaymentSystems.PaymentIQCryptoPay ||  paymentSystem.Name == Constants.PaymentSystems.PaymentIQInterac)
                                && paymentRequest.Type != (int)PaymentRequestTypes.Withdraw)
                            {
                                paymentRequest.Amount = Math.Abs(input.Amount);
                                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            }
                            if (client.Id.ToString() != input.UserId || paymentRequest.CurrencyId != input.Currency ||
                                paymentRequest.Amount != Math.Abs(input.Amount))
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                            var parameters = !string.IsNullOrEmpty(paymentRequest.Parameters) ?
                                      JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters) : new Dictionary<string, string>();
                            if (!string.IsNullOrEmpty(input.AccountId) && !parameters.ContainsKey("AccountId"))
                                parameters.Add("AccountId", input.AccountId);                          
                            paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);

                            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");     
                            paymentInfo.PSPRefId = input.PSPRefId;
                            paymentRequest.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Ignore
                            });
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            {
                                clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                            }
                            else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                            {
                                using (var notificationBl = new NotificationBll(paymentSystemBl))
                                {
                                    var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                                                                   null, null, false, paymentRequest.Parameters, documentBll, notificationBl, out List<int> userIds);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    foreach (var uId in userIds)
                                    {
                                        PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                    }
                                }
                            }
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            response.Success = true;
                            response.UserId = input.UserId;
                            response.MerchantTxId = paymentRequest.Id.ToString();
                            response.AuthCode = paymentRequest.Id.ToString();
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Detail.Message;
                response.ErrorCode = ex.Detail.Id;
                WebApiApplication.DbLogger.Error(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                response.ErrorCode = Constants.Errors.GeneralException;
                WebApiApplication.DbLogger.Error(ex);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
        [HttpPost]
        [Route("api/paymentiq/cancel")]
        public HttpResponseMessage CancelRequest(AuthorizeInput input)
        {
            var response = new AuthorizeOutput();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                var paymentRequest = paymentSystemBl.GetPaymentRequestById(input.Attributes.MerchantTransactionId);
                                if (paymentRequest == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                                if (client.Id.ToString() != input.UserId || paymentRequest.CurrencyId != input.Currency)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                                if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                                {
                                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, string.Empty, notificationBl);
                                }
                                else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                                {
                                    clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
                                                                        "canceled", null, null, false, string.Empty, documentBll, notificationBl, out List<int> userIds);
                                    foreach (var uId in userIds)
                                    {
                                        PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                    }
                                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                                }
                                response.Success = true;
                                response.UserId = input.UserId;
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Detail.Message;
                response.ErrorCode = ex.Detail.Id;
                WebApiApplication.DbLogger.Error(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                response.ErrorCode = Constants.Errors.GeneralException;
                WebApiApplication.DbLogger.Error(ex);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
    }
}