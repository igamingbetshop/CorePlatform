using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.CryptoPay;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class CryptoPayController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "63.33.129.150",
            "63.33.105.227"
        };

        [HttpPost]
        [Route("api/CryptoPay/ApiRequest")]
        public ActionResult ApiRequest(HttpRequestMessage input)
        {
            var response = string.Empty;
            try
            {
                var inputString = input.Content.ReadAsStringAsync().Result;
                Program.DbLogger.Info("inputString:" + inputString);
                //   BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.ContainsKey("X-Cryptopay-Signature"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var inputSignature = Request.Headers["X-Cryptopay-Signature"];
                if (string.IsNullOrEmpty(inputSignature))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var paymentInput = JsonConvert.DeserializeObject<PaymentInput>(inputString);
                if (paymentInput.Details.Status == "completed")
                {
                    if (paymentInput.Type.ToLower() == "channelpayment")
                        PaymentRequest(paymentInput, inputString, inputSignature);
                    else if (paymentInput.Type.ToLower() == "coinwithdrawal")
                        PayoutRequest(paymentInput, inputString, inputSignature);
                }
                response = "OK";
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
            }
            catch (Exception ex)
            {
                response = ex.Message;
                Program.DbLogger.Error(response);
            }
            return Ok(response);
        }
        private void PayoutRequest(PaymentInput paymentInput, string inputString, string inputSignature)
        {
            using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
            using var clientBl = new ClientBll(paymentSystemBl);
            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt32(paymentInput.Details.CustomId));
            if (paymentRequest == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
            var client = CacheManager.GetClientById(paymentRequest.ClientId);

            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                               client.CurrencyId, paymentRequest.Type);
            var sign = CommonFunctions.ComputeHMACSha256(inputString, partnerPaymentSetting.Password.Split(',')[1]);
            if (sign.ToLower() !=  inputSignature.ToLower())
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");
            var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
            JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
            parameters.Add("Callback_ExchangeDetails", JsonConvert.SerializeObject(paymentInput.Details.ExchangeDetails));
            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
            {
                parameters.Add("Fee", paymentInput.Details.Fee.ToString());
                parameters.Add("Fee Currency", paymentInput.Details.FeeCurrency.ToString());
                paymentInfo.PayAmount =Convert.ToDecimal(paymentInput.Details.PaidAmount);
                paymentRequest.Amount = paymentInput.Details.ReceivedAmount;
            }
            else
            {
                parameters.Add("Network Fee", paymentInput.Details.NetworkFee.ToString());
                paymentInfo.PayAmount =Convert.ToDecimal(paymentInput.Details.ReceivedAmount);
            }
            paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
            paymentRequest.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
            if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
            {
                using var documentBl = new DocumentBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                if (paymentInput.Details.Status.ToLower() == "completed")
                {
                    var req = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, paymentInput.Details.Status,
                                                                    null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
                    clientBl.PayWithdrawFromPaymentSystem(req, documentBl, notificationBl);
                }
                else if (paymentInput.Details.Status.ToLower() == "cancelled" || paymentInput.Details.Status.ToLower() == "refunded")
                    clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, paymentInput.Details.Status, null,
                                                        null, false, paymentRequest.Parameters, documentBl, notificationBl);
            }
        }

        private void PaymentRequest(PaymentInput paymentInput, string inputString, string inputSignature)
        {
            var client = CacheManager.GetClientById(Convert.ToInt32(paymentInput.Details.CustomId.Split('_')[0]));
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.CryptoPay + paymentInput.Details.PaidCurrency.ToUpper());
            if (paymentSystem == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id,
                                                                               client.CurrencyId, (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
            if (partnerPaymentSetting.State != (int)PartnerPaymentSettingStates.Active)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingBlocked);
            var sign = CommonFunctions.ComputeHMACSha256(inputString, partnerPaymentSetting.Password.Split(',')[1]);
            if (sign.ToLower() !=  inputSignature.ToLower())
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
            if (client.State == (int)ClientStates.BlockedForDeposit || client.State == (int)ClientStates.FullBlocked ||
            client.State == (int)ClientStates.Suspended || client.State == (int)ClientStates.Disabled)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientBlocked);
            if (paymentInput.Details.ReceivedAmount <= 0)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
            if (partnerPaymentSetting.MinAmount > paymentInput.Details.ReceivedAmount ||
                partnerPaymentSetting.MaxAmount < paymentInput.Details.ReceivedAmount)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestInValidAmount);
            var paymentRequest = new PaymentRequest
            {
                Amount = paymentInput.Details.ReceivedAmount,
                ClientId = client.Id,
                CurrencyId = client.CurrencyId,
                PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                PartnerPaymentSettingId = partnerPaymentSetting.Id,
                ExternalTransactionId = paymentInput.Details.TxId
            };

            using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
            using var clientBl = new ClientBll(paymentSystemBl);
            using var regionBl = new RegionBll(paymentSystemBl);
            using var notificationBl = new NotificationBll(paymentSystemBl);
            var regionPath = regionBl.GetRegionPath(client.RegionId);
            var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
            var cityPath = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City);
            var city = string.Empty;
            if (cityPath != null)
                city = CacheManager.GetRegionById(cityPath.Id ?? 0, client.LanguageId)?.Name;
            var paymentInfo = new PaymentInfo
            {
                Country = country?.IsoCode,
                City = city,
            };
            paymentRequest.CountryCode = country?.IsoCode;

            var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
            JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
            if (paymentInput.Details.ExchangeDetails != null && !parameters.ContainsKey("ExchangeDetails"))
                parameters.Add("ExchangeDetails", JsonConvert.SerializeObject(paymentInput.Details.ExchangeDetails));
            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
            {
                if (!parameters.ContainsKey("PaidAmount"))
                    parameters.Add("PaidAmount", paymentInput.Details.PaidAmount.ToString());
                if (!parameters.ContainsKey("PaidCurrency"))
                    parameters.Add("PaidCurrency", paymentInput.Details.PaidCurrency.ToString());
                paymentInfo.PayAmount = Convert.ToDecimal(paymentInput.Details.PaidAmount);
                paymentRequest.Amount = paymentInput.Details.ReceivedAmount;
            }
            else
            {
                if (!parameters.ContainsKey("Network Fee"))
                    parameters.Add("Network Fee", paymentInput.Details.NetworkFee.ToString());
                paymentInfo.PayAmount =Convert.ToDecimal(paymentInput.Details.ReceivedAmount);
            }
            paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
            paymentRequest.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                var request = clientBl.CreateDepositFromPaymentSystem(paymentRequest);
                PaymentHelpers.InvokeMessage("PaymentRequst", request.ClientId);
                if (paymentInput.Details.Status.ToLower() == "completed")
                    clientBl.ApproveDepositFromPaymentSystem(request, false, paymentInput.Details.Status);
                else if (paymentInput.Details.Status.ToLower() == "cancelled" || paymentInput.Details.Status.ToLower() == "refunded")
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, paymentInput.Details.Status, notificationBl);
                scope.Complete();
            }
            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
        }
    }
}