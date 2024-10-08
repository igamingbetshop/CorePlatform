using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Integration.Payments.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class IqWalletHelpers
    {
        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log, out List<int> userIds)
        {
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
            if (!Int32.TryParse(paymentInfo.WalletNumber, out int toClientId))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
            var toClient = CacheManager.GetClientById(toClientId) ??
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(toClient.PartnerId, paymentRequest.PaymentSystemId,
                                                                               toClient.CurrencyId, (int)PaymentRequestTypes.Deposit) ??
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingBlocked);
            var fromClient = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
            if (fromClient.CurrencyId != toClient.CurrencyId)
            {
                amount = BaseBll.ConvertCurrency(fromClient.CurrencyId, toClient.CurrencyId, amount);
                if (amount < partnerPaymentSetting.MinAmount || amount > partnerPaymentSetting.MaxAmount)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestInValidAmount);
            }
            if (toClient.State == (int)ClientStates.FullBlocked || toClient.State == (int)ClientStates.Disabled ||
            toClient.State == (int)ClientStates.Suspended || toClient.State == (int)ClientStates.SuspendedWithWithdraw ||
            toClient.State == (int)ClientStates.BlockedForDeposit)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientBlocked);

            using (var clientBl = new ClientBll(session, log))
            using (var documentBl = new DocumentBll(clientBl))
            using (var notificationBl = new NotificationBll(clientBl))
            {
                clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.PayPanding, string.Empty,
                                                    null, null, true, paymentRequest.Parameters, documentBl, notificationBl, out userIds, false);
            }
            return new PaymentResponse
            {
                Status = PaymentRequestStates.PayPanding,
                Description = "Pay Panding"
            };
        }

        public static List<int> ApprovePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log, out int toClientId)
        {
            var userIds = new List<int>();
            toClientId = 0;
            if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw && paymentRequest.Status == (int)PaymentRequestStates.PayPanding)
            {
                try
                {
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                    if (!Int32.TryParse(paymentInfo.WalletNumber, out toClientId))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                    var toClient = CacheManager.GetClientById(toClientId) ??
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                    var fromClient = CacheManager.GetClientById(paymentRequest.ClientId.Value);

                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(toClient.PartnerId, paymentRequest.PaymentSystemId,
                                                                                       toClient.CurrencyId, (int)PaymentRequestTypes.Deposit) ??
                 throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                    if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingBlocked);
                    if (toClient.State == (int)ClientStates.FullBlocked || toClient.State == (int)ClientStates.Disabled ||
                    toClient.State == (int)ClientStates.Suspended || toClient.State == (int)ClientStates.SuspendedWithWithdraw ||
                    toClient.State == (int)ClientStates.BlockedForDeposit)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientBlocked);
                    var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                               JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                    var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                    if (fromClient.CurrencyId != toClient.CurrencyId)
                    {
                        var rate = BaseBll.GetCurrenciesDifference(fromClient.CurrencyId, toClient.CurrencyId);
                        amount = Math.Round(rate * paymentRequest.Amount, 2);
                        if (!parameters.ContainsKey("Currency"))
                            parameters.Add("Currency", toClient.CurrencyId);
                        else
                            parameters["Currency"] = toClient.CurrencyId;
                        if (!parameters.ContainsKey("TransferedAmount"))
                            parameters.Add("TransferedAmount", amount.ToString("F"));
                        else
                            parameters["TransferedAmount"] = amount.ToString("F");
                        if (!parameters.ContainsKey("AppliedRate"))
                            parameters.Add("AppliedRate", rate.ToString("F"));
                        else
                            parameters["AppliedRate"] = rate.ToString("F");
                    }
                    if (amount < partnerPaymentSetting.MinAmount || amount > partnerPaymentSetting.MaxAmount)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestInValidAmount);

                    using (var transactionScope = CommonFunctions.CreateTransactionScope())
                    {
                        using (var clientBl = new ClientBll(session, log))
                        using (var documentBl = new DocumentBll(clientBl))
                        using (var paymentSystemBl = new PaymentSystemBll(clientBl))
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved,
                                               string.Empty, null, null, false, paymentRequest.Parameters, documentBl, notificationBl, out userIds);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);

                            // creating deposit request for reciver
                            var receiverPaymentRequest = new PaymentRequest
                            {
                                Type = (int)PaymentRequestTypes.Deposit,
                                Amount = amount,
                                ClientId = toClient.Id,
                                CurrencyId = toClient.CurrencyId,
                                PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                                PartnerPaymentSettingId = partnerPaymentSetting.Id,
                                ExternalTransactionId = paymentRequest.Id.ToString(),
                                Parameters = "{}",
                                Info = "{}"
                            };
                            var receiverRequest = clientBl.CreateDepositFromPaymentSystem(receiverPaymentRequest, out LimitInfo info, false);
                            paymentRequest.ExternalTransactionId = receiverRequest.Id.ToString();
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            clientBl.ApproveDepositFromPaymentSystem(receiverRequest, false, out userIds, "Approved");
                            transactionScope.Complete();
                            CacheManager.RemoveClientBalance(toClientId);
                            CacheManager.RemoveClientBalance(fromClient.Id);
                        }
                    }
                }
                catch (FaultException<BllFnErrorType> ex)
                {
                    log.Error(ex);
                    using (var clientBl = new ClientBll(session, log))
                    using (var documentBl = new DocumentBll(clientBl))
                    using (var notificationBl = new NotificationBll(clientBl))
                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, ex.Detail.Message,
                                         null, null, false, paymentRequest.Parameters, documentBl, notificationBl, out userIds);
                    throw;
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    using (var clientBl = new ClientBll(session, log))
                    using (var documentBl = new DocumentBll(clientBl))
                    using (var notificationBl = new NotificationBll(clientBl))

                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, ex.Message,
                                                            null, null, false, paymentRequest.Parameters, documentBl, notificationBl, out userIds);
                    throw;
                }
            }
            return userIds;
        }
    }
}