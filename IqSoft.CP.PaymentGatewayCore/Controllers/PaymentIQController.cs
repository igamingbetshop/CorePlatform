using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Models.PaymentIQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class PaymentIQController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "54.229.9.44",
            "54.229.9.44",
            "54.194.243.247",
            "52.51.194.179",
            "52.209.182.232",
            "34.241.202.249",
            "52.19.173.50"
        };

        [HttpPost]
        [Route("api/paymentiq/verifyuser")]
        public ActionResult VerifyUser(VerifyUserInput input)
        {
            var response = new VerifyOutput();
            var userIp = string.Empty;
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var client = CacheManager.GetClientById(input.ClientId);
                if (client== null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var regionBl = new RegionBll(paymentSystemBl);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.SessionId));
                if (paymentRequest == null ||
                   //(paymentRequest.Type ==  (int)PaymentRequestTypes.Deposit && paymentRequest.Status != (int)PaymentRequestStates.Pending) ||
                   (paymentRequest.Type ==  (int)PaymentRequestTypes.Withdraw && paymentRequest.Status != (int)PaymentRequestStates.Confirmed))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var clientSession = paymentSystemBl.GetClientSessionById(paymentRequest.SessionId ?? 0);
                var region = regionBl.GetRegionByCountryCode(clientSession != null ? clientSession.Country : "SE");
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                userIp = paymentInfo.TransactionIp;
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
            catch (FaultException<BllFnErrorType> ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Detail.Message;
                response.ErrorCode = ex.Detail.Id;
                Program.DbLogger.Error(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                response.ErrorCode = Constants.Errors.GeneralException;
                Program.DbLogger.Error(ex);
            }
            Response.Headers.Add("PIQ-Client-IP", userIp);
            return Ok(response);
        }

        [HttpPost]
        [Route("api/paymentiq/authorize")]
        public ActionResult AuthorizeUser(AuthorizeInput input)
        {
            var response = new AuthorizeOutput();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);

                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(input.Attributes.MerchantTransactionId);
                if (paymentRequest== null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                if (client.Id.ToString() != input.UserId || paymentRequest.CurrencyId != input.Currency ||
                   (paymentRequest.Amount != Math.Abs(input.Amount) && paymentSystem.Name != Constants.PaymentSystems.PaymentIQCryptoPay))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                paymentRequest.ExternalTransactionId = input.TransactionId;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                response.Success = true;
                response.UserId = input.UserId;
                response.MerchantTxId = paymentRequest.Id.ToString();
                response.AuthCode = paymentRequest.Id.ToString();
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Detail.Message;
                response.ErrorCode = ex.Detail.Id;
                Program.DbLogger.Error(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                response.ErrorCode = Constants.Errors.GeneralException;
                Program.DbLogger.Error(ex);
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("api/paymentiq/transfer")]
        public ActionResult TransferRequest(AuthorizeInput input)
        {
            var response = new AuthorizeOutput();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var documentBll = new DocumentBll(paymentSystemBl);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(input.Attributes.MerchantTransactionId);
                if (paymentRequest== null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                if (paymentSystem.Name == Constants.PaymentSystems.PaymentIQCryptoPay || paymentSystem.Name == Constants.PaymentSystems.PaymentIQInterac)
                {
                    paymentRequest.Amount = Math.Abs(input.Amount);
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                }
                if (client.Id.ToString() != input.UserId || paymentRequest.CurrencyId != input.Currency ||
                    paymentRequest.Amount != Math.Abs(input.Amount))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                {
                    if (!string.IsNullOrEmpty(input.AccountId))
                    {
                        var parameters = !string.IsNullOrEmpty(paymentRequest.Parameters) ?
                           JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters) : new Dictionary<string, string>();
                        if (!parameters.ContainsKey("AccountId"))
                        {
                            parameters.Add("AccountId", input.AccountId);
                            paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                        }
                    }
                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);                   
                }
                else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                {
                    using (var notificationBl = new NotificationBll(paymentSystemBl))
                    {
                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                                                       null, null, false, paymentRequest.Parameters, documentBll, notificationBl);
                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                    }
                }
                response.Success = true;
                response.UserId = input.UserId;
                response.MerchantTxId = paymentRequest.Id.ToString();
                response.AuthCode = paymentRequest.Id.ToString();
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Detail.Message;
                response.ErrorCode = ex.Detail.Id;
                Program.DbLogger.Error(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                response.ErrorCode = Constants.Errors.GeneralException;
                Program.DbLogger.Error(ex);
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("api/paymentiq/cancel")]
        public ActionResult CancelRequest(AuthorizeInput input)
        {
            var response = new AuthorizeOutput();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var documentBll = new DocumentBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(input.Attributes.MerchantTransactionId);
                if (paymentRequest== null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
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
                                                        "canceled", null, null, false, string.Empty, documentBll, notificationBl);
                }
                response.Success = true;
                response.UserId = input.UserId;
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Detail.Message;
                response.ErrorCode = ex.Detail.Id;
                Program.DbLogger.Error(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                response.ErrorCode = Constants.Errors.GeneralException;
                Program.DbLogger.Error(ex);
            }
            return Ok(response);
        }
    }
}