using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Helpers;
using IqSoft.CP.AgentWebApi.Helpers;
using IqSoft.CP.AgentWebApi.Filters;
using IqSoft.CP.AgentWebApi.Models;
using IqSoft.CP.AgentWebApi.Models.Payment;
using log4net;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using IqSoft.CP.DAL;

namespace IqSoft.CP.AgentWebApi.ControllerClasses
{
    public static class PaymentController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetPaymentRequestsPaging":
                    return GetPaymentRequestsPaging(
                            JsonConvert.DeserializeObject<ApiFilterfnPaymentRequest>(request.RequestData), identity, log);
                case "GetEntryList":
                    return GetEntryList(JsonConvert.DeserializeObject<PaymentModel>(request.RequestData), identity, log);
                case "RejectPaymentRequest":
                    return RejectPaymentRequest(JsonConvert.DeserializeObject<ChangePaymentRequestState>(request.RequestData), identity, log);
                case "UpdatePaymentEntry":
                    return UpdateEntry(JsonConvert.DeserializeObject<ApiUpdatePaymentEntryInput>(request.RequestData), identity, log);
                case "GetPaymentSystems":
                    return GetPaymentSystems(identity, log);
                case "GetPaymentRequestHistories":
                    return GetPaymentRequestHistories(JsonConvert.DeserializeObject<GetPaymentRequestHistoriesInput>(request.RequestData), identity, log);
                case "GetPaymentRequestById":
                    return GetPaymentRequestById(Convert.ToInt64(request.RequestData), identity, log);
                case "PayPaymentRequest":
                    return PayPaymentRequest(Convert.ToInt64(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.MethodNotFound);
        }

        public static ApiResponseBase GetEntryList(PaymentModel paymentModel, SessionIdentity identity, ILog log)
        {
            var response = new ApiResponseBase
            {
                ResponseObject = SerosPayHelpers.GetPaymentEntries(paymentModel.PaymentSystemIds[0], paymentModel.PartnerId, identity, log)
            };
            return response;
        }

        private static ApiResponseBase GetPaymentRequestsPaging(ApiFilterfnPaymentRequest filter, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var agent = CacheManager.GetUserById(identity.Id);
                var isAgentEmploye = agent.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                {
                    paymentSystemBl.CheckPermission(Constants.Permissions.ViewPaymentRequests);
                    filter.AgentId = agent.ParentId;
                }
                else
                    filter.AgentId = identity.Id;
                var input = filter.MapToFilterfnPaymentRequest();
                input.WithPendings = null;
                var resp = paymentSystemBl.GetPaymentRequestsPaging(input, true, false).MapToApiPaymentRequestsReport(paymentSystemBl.GetUserIdentity().TimeZone);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        PaymentRequests = resp
                    }
                };
                return response;
            }
        }

        public static ApiResponseBase RejectPaymentRequest(ChangePaymentRequestState request, SessionIdentity identity, ILog log)
        {
            //checkPermission
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var documentBl = new DocumentBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var r = paymentSystemBl.GetPaymentRequestById(request.PaymentRequestId);
                            if (r == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);

                            if (r.Type == (int)PaymentRequestTypes.Deposit)
                                clientBl.CancelDeposit(request.PaymentRequestId, request.Comment);
                            else
                            {
                                clientBl.ChangeWithdrawRequestState(
                                    request.PaymentRequestId,
                                    PaymentRequestStates.CanceledByUser,
                                    request.Comment,
                                    request.CashDeskId,
                                    null, true, string.Empty, documentBl, notificationBl, out List<int> userIds);

                                foreach (var uId in userIds)
                                {
                                    Helpers.Helpers.InvokeMessage("NotificationsCount", uId);
                                }
                            }
                            CacheManager.RemoveClientBalance(r.ClientId.Value);
                            CacheManager.RemoveClientDepositCount(r.ClientId.Value);
                            Helpers.Helpers.InvokeMessage("ClientDeposit", r.ClientId);
                            return new ApiResponseBase();
                        }
                    }
                }
            }
        }
        public static ApiResponseBase UpdateEntry(ApiUpdatePaymentEntryInput entryField, SessionIdentity identity, ILog log)
        {            
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            using (var clientBl = new ClientBll(paymentSystemBl))
            using (var userBl = new UserBll(paymentSystemBl))
            using (var notificationBl = new NotificationBll(paymentSystemBl))
            using (var documentBl = new DocumentBll(paymentSystemBl))
            {
                var user = CacheManager.GetUserById(identity.Id);
                var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                if (isAgentEmploye)
                    userBl.CheckPermission(Constants.Permissions.PayPaymentRequest);
                var request = paymentSystemBl.GetPaymentRequestById(entryField.PaymentRequestId);
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId.Value);
                if(!client.UserId.HasValue ||
                   (user.Type == (int)UserTypes.AgentEmployee && client.UserId != user.ParentId ) ||
                   (user.Type != (int)UserTypes.AgentEmployee && client.UserId != user.Id))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.NotAllowed);

                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                   client.CurrencyId, request.Type);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);

                var correctionInput = new ClientCorrectionInput
                {
                    Amount = request.Amount,
                    AccountTypeId = (int)AccountTypes.ClientUnusedBalance,
                    CurrencyId = request.CurrencyId,
                    ClientId = request.ClientId.Value
                };
                Document result;
                if (request.Type ==  (int)PaymentRequestTypes.Deposit)
                     result = clientBl.CreateDebitCorrectionOnClient(correctionInput, documentBl, false);
                else
                    result = clientBl.CreateCreditCorrectionOnClient(correctionInput, documentBl, false);
                request.ExternalTransactionId = result.Id.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(request);
                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.ApprovedManually, string.Empty, notificationBl);
                Helpers.Helpers.InvokeMessage("ClientDepositWithBonus", request.ClientId);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, client.Id));
                return new ApiResponseBase();
            }
        }

        public static ApiResponseBase PayPaymentRequest(long paymentRequestId, SessionIdentity identity, ILog log)
        {
            var userIds = new List<int>();
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            using (var clientBl = new ClientBll(paymentSystemBl))
            {
                var user = CacheManager.GetUserById(identity.Id);
                if (user.Type == (int)UserTypes.AdminUser)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                if (user.Type == (int)UserTypes.AgentEmployee)
                {
                    paymentSystemBl.CheckPermission(Constants.Permissions.ViewClient);
                    paymentSystemBl.CheckPermission(Constants.Permissions.PayPaymentRequest);
                    user = CacheManager.GetUserById(user.ParentId.Value);
                }
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(paymentRequestId) ??
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                if (client == null || client.UserId != user.Id)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);

                var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                if ((paymentRequest.Type == (int)PaymentRequestTypes.Deposit || paymentRequest.Type == (int)PaymentRequestTypes.ManualDeposit) &&
                    Constants.ManualPaymentSystems.Contains(paymentSystem.Name))
                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds, comment: "Manually Approved From Agent System");
                else
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);
                Helpers.Helpers.InvokeMessage("ClientDepositWithBonus", client.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, client.Id));
                foreach (var uId in userIds)
                    Helpers.Helpers.InvokeMessage("NotificationsCount", uId);

                return new ApiResponseBase();
            }
        }

        public static ApiResponseBase GetPaymentSystems(SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var result = paymentSystemBl.GetPaymentSystems(null);
                return new ApiResponseBase
                {
                    ResponseObject =
                        result.Select(
                            x => x.MapToPaymentSystemModel(identity.TimeZone))
                };
            }
        }
      
        public static ApiResponseBase GetPaymentRequestHistories(GetPaymentRequestHistoriesInput request, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var response = new ApiResponseBase
                {
                    ResponseObject = paymentSystemBl.GetPaymentRequestHistories(new List<long> { request.PaymentRequestId }).MapToPaymentRequestHistoryModels(paymentSystemBl.GetUserIdentity().TimeZone)
                };
                return response;
            }
        }

        public static ApiResponseBase GetPaymentRequestById(long requestId, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var result = paymentSystemBl.GetfnPaymentRequestById(requestId).MapToApiPaymentRequest(paymentSystemBl.GetUserIdentity().TimeZone);
                var client = CacheManager.GetClientById(result.ClientId);
                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        PaymentRequest = result,
                        ClientMobileVerified = client.IsMobileNumberVerified,
                        ClientEmailVerified = client.IsEmailVerified,
                        ClientDocumentVerified = client.IsDocumentVerified
                    }
                };
                return response;
            }
        }
    }
}