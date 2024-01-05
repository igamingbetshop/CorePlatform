using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.PaymentGateway.Models.IqWallet;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using System.Text;
using System.ServiceModel;
using System;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class IqWalletControllerController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "188.40.107.26",
            "138.201.48.200"
        };

        [HttpPost]
        [Route("api/IqWallet/PaymentRequest")]
        public ActionResult PaymentRequest(PaymentRequestInput input)
        {
            var response = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    try
                    {
                        var ip = string.Empty;
                        if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                            ip = header.ToString();
                        BaseBll.CheckIp(WhitelistedIps, ip);
                        var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MerchantPaymentId));
                        if (request == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(request.ClientId);
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                            request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                        var signature = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedValuesAsString(input, ",") + partnerPaymentSetting.Password);

                        if (input.Sign.ToLower() != signature.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                        if (input.Status == (int)PaymentRequestStates.Approved)
                        {
                            request.ExternalTransactionId = input.PaymentId.ToString();
                            paymentSystemBl.ChangePaymentRequestDetails(request);
                            var doc = clientBl.ApproveDepositFromPaymentSystem(request, false);
                            return Conflict(new StringContent("OK", Encoding.UTF8));
                        }
                        else
                            return Conflict(new StringContent("Error", Encoding.UTF8));
                    }
                    catch (FaultException<BllFnErrorType> ex)
                    {
                        if (ex.Detail != null &&
                            (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                            ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                        {
                            return Ok(new StringContent("OK", Encoding.UTF8));
                        }
                        var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

                        Program.DbLogger.Error(exp);
                        return Conflict(new StringContent(exp.Message, Encoding.UTF8));
                    }
                    catch (Exception ex)
                    {
                        Program.DbLogger.Error(ex);
                    }
                    return Conflict(new StringContent("", Encoding.UTF8));

                }
            }
        }

        [HttpPost]
        [Route("api/IqWallet/PayoutRequest")]
        public ActionResult PayoutRequest(PaymentRequestInput input)
        {
            var response = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var documentBl = new DocumentBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            try
                            {
                                var ip = string.Empty;
                                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                                    ip = header.ToString();
                                BaseBll.CheckIp(WhitelistedIps, ip);
                                var request = paymentSystemBl.GetPaymentRequestById(System.Convert.ToInt64(input.MerchantPaymentId));
                                if (request == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId);
                                if (client == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);

                                var signature = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedValuesAsString(input, ",") + partnerPaymentSetting.Password);

                                if (input.Sign.ToLower() != signature.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                                if (input.Status == (int)PaymentRequestStates.Approved)
                                {
                                    request.ExternalTransactionId = input.PaymentId.ToString();
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty, request.CashDeskId,
                                                                                    null, true, request.Parameters, documentBl, notificationBl);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                                    return Ok(new StringContent("OK", Encoding.UTF8));
                                }
                                else
                                    return Conflict(new StringContent("Error", Encoding.UTF8));
                            }
                            catch (FaultException<BllFnErrorType> ex)
                            {
                                if (ex.Detail != null &&
                                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                                {
                                    return Ok(new StringContent("OK", Encoding.UTF8));
                                }
                                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

                                Program.DbLogger.Error(exp);
                                return Conflict(new StringContent(exp.Message, Encoding.UTF8));
                            }
                            catch (Exception ex)
                            {
                                Program.DbLogger.Error(ex);
                            }
                            return Conflict(new StringContent("", Encoding.UTF8));
                        }
                    }
                }
            }
        }
    }
}