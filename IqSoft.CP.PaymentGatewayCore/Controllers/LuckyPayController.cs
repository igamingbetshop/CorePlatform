using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.LuckyPay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class LuckyPayController : ControllerBase
    {
        private BllPaymentSystem _paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.LuckyPay);
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "35.173.47.95",
            "3.229.158.115"
        };

        [HttpPost]
        [Route("api/LuckyPay/Check")]
        public ActionResult WithdrawResult(PaymentStatusInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var client = CacheManager.GetClientByMobileNumber(input.partnerId, input.mobileNumber);
                if (client.Id > 0)
                {
                    var response = JsonConvert.SerializeObject(new { Id = client.Id, FirstName = client.FirstName, LastName = client.LastName, CurrencyId = client.CurrencyId });

                    return Ok(new StringContent(response, Encoding.UTF8));
                }

                return BadRequest();
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex);
                return BadRequest();
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("api/LuckyPay/Deposit")]
        public ActionResult ApiRequest(PaymentStatusInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                if (input.amount <= 0)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
                var client = CacheManager.GetClientById(input.clientId);
                if (client == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
                var clientState = client.State;
                if (clientState == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientBlocked);
                PaymentRequest paymentRequest = null;
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var scope = CommonFunctions.CreateTransactionScope())
                        {
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, _paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            if (partnerPaymentSetting == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                            if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);
                            paymentRequest = paymentSystemBl.GetPaymentRequest(input.transactionId.ToString(), _paymentSystem.Id, (int)PaymentRequestTypes.Deposit);

                            if (paymentRequest != null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestAlreadyExists);

                            paymentRequest = clientBl.CreateDepositFromPaymentSystem(new PaymentRequest
                            {
                                ClientId = client.Id,
                                Amount = input.amount,
                                CurrencyId = client.CurrencyId,
                                PaymentSystemId = _paymentSystem.Id,
                                PartnerPaymentSettingId = partnerPaymentSetting.Id,
                                ExternalTransactionId = input.transactionId.ToString(),
                                Info = input.transactionId.ToString()
                            });
                            PaymentHelpers.InvokeMessage("PaymentRequst", paymentRequest.ClientId);
                            clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                            scope.Complete();
                        }
                    }
                }
                var response = new
                {
                    ClientId = input.clientId,
                    TransactionId = paymentRequest.Id,
                    Amount = input.amount
                };

                return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex);
                return BadRequest();
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return BadRequest();
            }
        }
    }
}
