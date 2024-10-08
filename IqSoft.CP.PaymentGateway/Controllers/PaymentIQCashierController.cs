using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.PaymentIQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class PaymentIQCashierController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.PaymentIQ);
        private static readonly BllPaymentSystem PaymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PaymentIQ);

        [HttpPost]
        [Route("api/paymentiqcashier/verifyuser")]
        public HttpResponseMessage VerifyUser(VerifyUserInput input)
        {
            var response = new VerifyOutput();
            var userIp = string.Empty;
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientById(input.ClientId);
                if (client== null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var authToken = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, PaymentSystem.Id, Constants.PartnerKeys.PaymentIQAuthToken);
                CheckAuthentication(authToken);
                var clientSession = CacheManager.GetClientSessionByToken(input.SessionId, Constants.PlatformProductId);
                if (clientSession.ClientId != input.ClientId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                if (client.State == (int)ClientStates.BlockedForDeposit || client.State == (int)ClientStates.FullBlocked ||
                    client.State == (int)ClientStates.Suspended || client.State == (int)ClientStates.SuspendedWithWithdraw ||
                    client.State == (int)ClientStates.Disabled)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientBlocked);
                var regionPath = CacheManager.GetRegionPathById(client.RegionId);
                var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
                var city = client.City;
                if (string.IsNullOrEmpty(city))
                {
                    var cityPath = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City);
                    if (cityPath != null)
                        city = CacheManager.GetRegionById(cityPath.Id ?? 0, client.LanguageId)?.Name;
                }
                //var stateId = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.State)?.Id;
                //if (!stateId.HasValue)
                //    stateId = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country)?.Id;

                userIp = clientSession.Ip;
                response.UserId= client.Id.ToString();
                response.UserCurrency = client.CurrencyId;
                response.Balance = Math.Round(ClientBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2);
                response.FirstName = client.FirstName;
                response.LastName = client.LastName;
                response.UserCat =  CacheManager.GetClientCategory(client.CategoryId).Name;
                response.KycStatus = client.IsDocumentVerified ? "Approved" : "Pending";
                response.Gender =  ((Gender)client.Gender).ToString().ToUpper();
                response.Street = client.Address;
                response.City = city;
               // response.State = stateId.HasValue ? CacheManager.GetRegionById(stateId.Value, client.LanguageId).Name : string.Empty;
                response.Country = country.IsoCode3;
                response.Zip = string.IsNullOrEmpty(client.ZipCode?.Trim()) ? client.Id.ToString() : client.ZipCode.Trim();
                response.Email = client.Email;
                response.Dob = client.BirthDate.ToString("yyyy-MM-dd");
                response.MobileNumber =  client.MobileNumber;
                response.Locale = Integration.CommonHelpers.LanguageISOCodes[clientSession != null ? clientSession.LanguageId : "sw"];
                response.Success = true;
                response.Attributes = new AttributeModel { SessionId = input.SessionId };
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
        [Route("api/paymentiqcashier/authorize")]
        public HttpResponseMessage AuthorizeUser(AuthorizeInput input)
        {
            var response = new AuthorizeOutput();
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = CacheManager.GetClientSessionByToken(input.Attributes.SessionId, Constants.PlatformProductId);
                if (clientSession.ClientId.ToString() != input.UserId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var client = CacheManager.GetClientById(Convert.ToInt32(input.UserId));
                if (client== null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var authToken = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, PaymentSystem.Id, Constants.PartnerKeys.PaymentIQAuthToken);
                CheckAuthentication(authToken);
                var paymentRequestType = 0;
                if (input.TxName.ToLower().Contains("deposit"))
                    paymentRequestType = (int)PaymentRequestTypes.Deposit;
                else if (input.TxName.ToLower().Contains("withdraw"))
                    paymentRequestType = (int)PaymentRequestTypes.Withdraw;
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, PaymentSystem.Id,
                                                                                   client.CurrencyId, paymentRequestType);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                if (client.State == (int)ClientStates.BlockedForDeposit || client.State == (int)ClientStates.FullBlocked ||
                    client.State == (int)ClientStates.Suspended || client.State == (int)ClientStates.SuspendedWithWithdraw ||
                    client.State == (int)ClientStates.Disabled)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientBlocked);
                if ((input.Amount <= 0 && paymentRequestType == (int)PaymentRequestTypes.Deposit)  ||
                    (input.Amount >= 0 && paymentRequestType == (int)PaymentRequestTypes.Withdraw) ||
                     input.Currency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                var paymentRequest = new PaymentRequest
                {
                    Amount = input.Amount,
                    ClientId = client.Id,
                    CurrencyId = client.CurrencyId,
                    PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                    PartnerPaymentSettingId = partnerPaymentSetting.Id,
                    ExternalTransactionId = input.TransactionId,
                    ActivatedBonusType = input.Attributes.BonusCode
                };
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity { SessionId = clientSession.Id }, WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var documentBl = new DocumentBll(clientBl))
                using (var notificationBl = new NotificationBll(clientBl))
                {
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");
                    paymentInfo.MaskedAccount = input.MaskedAccount;
                    paymentInfo.BankAccountHolder = input.AccountHolder;
                    paymentInfo.TxTypeId = input.TxTypeId;
                    paymentInfo.TxName = input.TxName;
                    paymentInfo.Provider = input.Provider;
                    paymentInfo.PSPService = input.PSPService;
                    paymentInfo.TransactionIp = clientSession.Ip;
                    paymentInfo.Country = clientSession.Country;
                    paymentInfo.PromoCode = input.Attributes.PromoCode;
                    var info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                    if (paymentRequestType == (int)PaymentRequestTypes.Deposit)
                    {
                        paymentRequest = clientBl.CreateDepositFromPaymentSystem(paymentRequest, out LimitInfo limitInfo);
                    }
                    else if (paymentRequestType == (int)PaymentRequestTypes.Withdraw)
                    {
                        var clientSetting = CacheManager.GetClientSettingByName(client.Id, Constants.ClientSettings.UnusedAmountWithdrawPercent);
                        var partner = CacheManager.GetPartnerById(client.PartnerId);
                        var uawp = partner.UnusedAmountWithdrawPercent;
                        if (clientSetting != null && clientSetting.Id > 0 && clientSetting.NumericValue != null)
                            uawp = clientSetting.NumericValue.Value;
                        var paymentRequestModel = new PaymentRequestModel
                        {
                            ClientId = client.Id,
                            PartnerId = client.PartnerId,
                            Amount = Math.Abs(input.Amount),
                            CurrencyId = client.CurrencyId,
                            PaymentSystemId = PaymentSystem.Id,
                            Type = (int)PaymentRequestTypes.Withdraw,
                            Info = info,
                            Parameters = new Dictionary<string, string> { { "SessionId", clientSession.Id.ToString() } }
                        };

                        paymentRequest = clientBl.CreateWithdrawPaymentRequest(paymentRequestModel, uawp, client, documentBl, notificationBl);
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                    }
                    paymentRequest.Info = info;
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
        [Route("api/paymentiqcashier/transfer")]
        public HttpResponseMessage TransferRequest(AuthorizeInput input)
        {
            var response = new AuthorizeOutput();
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                BaseBll.CheckIp(WhitelistedIps);
                var userIds = new List<int>();
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestByExternalId(input.TransactionId, PaymentSystem.Id);
                            if (paymentRequest== null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var authToken = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, PaymentSystem.Id, Constants.PartnerKeys.PaymentIQAuthToken);
                            CheckAuthentication(authToken);
                            if (paymentRequest.Type != (int)PaymentRequestTypes.Withdraw)
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
                                clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds);
                            }
                            else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                            {
                                using (var notificationBl = new NotificationBll(paymentSystemBl))
                                {
                                    var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                                    if (paymentSystem.Name == Constants.PaymentSystems.PaymentIQ)
                                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.PayPanding, string.Empty,
                                                           null, null, false, paymentRequest.Parameters, documentBll, notificationBl, out userIds);
                                    var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                                                                   null, null, false, paymentRequest.Parameters, documentBll, notificationBl, out userIds);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                }
                            }
                            foreach (var uId in userIds)
                            {
                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
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
        [Route("api/paymentiqcashier/cancel")]
        public HttpResponseMessage CancelRequest(AuthorizeInput input)
        {
            var response = new AuthorizeOutput();
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
                                var paymentRequest = paymentSystemBl.GetPaymentRequestByExternalId(input.TransactionId, PaymentSystem.Id);
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
                                    var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                                        JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                                    var status = PaymentRequestStates.Failed;
                                    if (parameters.ContainsKey("CanceledBy"))
                                        status = parameters["CanceledBy"] == ((int)ObjectTypes.Client).ToString() ? PaymentRequestStates.CanceledByClient : 
                                                                                                                    PaymentRequestStates.CanceledByUser;
                                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, status, status.ToString(), null, null, false, 
                                                                            paymentRequest.Parameters, documentBll, notificationBl, out List<int> userIds);
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

        private void CheckAuthentication(string token)
        {
            if (!Request.Headers.Contains("Authorization"))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            var inputToken = Request.Headers.GetValues("Authorization").FirstOrDefault();
            if (string.IsNullOrEmpty(inputToken))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            if ("Bearer " + token != inputToken)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
        }
    }
}