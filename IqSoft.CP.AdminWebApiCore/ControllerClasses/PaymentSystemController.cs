﻿using System;
using System.Linq;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Filters.PaymentRequests;
using IqSoft.CP.AdminWebApi.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.AdminWebApi.Models.PaymentModels;
using IqSoft.CP.Integration.Payments.Helpers;
using log4net;
using IqSoft.CP.Common.Helpers;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.DAL.Models.Cache;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.AdminWebApi.Models.PartnerModels;
using Microsoft.AspNetCore.Hosting;
using IqSoft.CP.DAL.Models.Notification;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class PaymentSystemController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            switch (request.Method)
            {
                case "GetPaymentSystems":
                    return GetPaymentSystems(identity, log);
                case "GetPartnerPaymentSettings":
                    return GetPartnerPaymentSettings(
                            JsonConvert.DeserializeObject<ApiFilterfnPartnerPaymentSetting>(request.RequestData),
                            identity, log);
                case "GetPartnerPaymentSettingById":
                    return GetPartnerPaymentSettingById(Convert.ToInt32(request.RequestData), identity, log);
                case "SavePaymentSystem":
                    return SavePaymentSystem(JsonConvert.DeserializeObject<PaymentSystemModel>(request.RequestData),
                        identity, log);
                case "SavePartnerPaymentCurrencyRate":
                    return SavePartnerPaymentCurrencyRate(
                            JsonConvert.DeserializeObject<ApiPartnerPaymentCurrencyRate>(request.RequestData), identity, log);
                case "GetPartnerPaymentCurrencyRates":
                    return GetPartnerPaymentCurrencyRates(Convert.ToInt32(request.RequestData), identity, log);
                case "UpdatePartnerPaymentSetting":
                    return
                        UpdatePartnerPaymentSetting(
                            JsonConvert.DeserializeObject<ApiPartnerPaymentSetting>(request.RequestData), identity, log);
                case "AddPartnerPaymentSetting":
                    return
                        AddPartnerPaymentSetting(
                            JsonConvert.DeserializeObject<ApiPartnerPaymentSetting>(request.RequestData), identity, log);
                case "GetPaymentRequestsPaging":
                    return
                        GetPaymentRequestsPaging(
                            JsonConvert.DeserializeObject<ApiFilterfnPaymentRequest>(request.RequestData), identity, log);
                case "GetPaymentRequestById":
                    return
                        GetPaymentRequestById(Convert.ToInt64(request.RequestData), identity, log);
                case "RejectPaymentRequest":
                    return
                        RejectPaymentRequest(
                            JsonConvert.DeserializeObject<ChangePaymentRequestState>(request.RequestData), identity, log);
                case "SetToInProcessPaymentRequest":
                    return
                        SetToInProcessPaymentRequest(
                            JsonConvert.DeserializeObject<ChangePaymentRequestState>(request.RequestData), identity, log);
                case "SetToFrozenPaymentRequest":
                    return
                        SetToFrozenPaymentRequest(
                            JsonConvert.DeserializeObject<ChangePaymentRequestState>(request.RequestData), identity, log);
                case "SetToWaitingForKYCPaymentRequest":
                    return
                        SetToWaitingForKYCPaymentRequest(
                            JsonConvert.DeserializeObject<ChangePaymentRequestState>(request.RequestData), identity, log);
                case "AllowPaymentRequest":
                    return
                        AllowPaymentRequest(
                            JsonConvert.DeserializeObject<ChangePaymentRequestState>(request.RequestData), identity, log);
                case "PayPaymentRequest":
                    return
                        PayPaymentRequest(
                            JsonConvert.DeserializeObject<ChangePaymentRequestState>(request.RequestData), identity, log);
                case "GetPaymentRequestHistories":
                    return
                        GetPaymentRequestHistories(
                            JsonConvert.DeserializeObject<GetPaymentRequestHistoriesInput>(request.RequestData),
                            identity, log);
                case "ExportPaymentRequests":
                    var filter = JsonConvert.DeserializeObject<ApiFilterfnPaymentRequest>(request.RequestData);
                    if (filter.Type == (int)PaymentRequestTypes.Deposit)
                        return ExportDepositPaymentRequests(filter, identity, log, env);
                    if (filter.Type == (int)PaymentRequestTypes.Withdraw)
                        return ExportWithdrawalPaymentRequests(filter, identity, log, env);
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                case "ExportDepositPaymentRequests":// should be removed
                    return
                        ExportDepositPaymentRequests(
                            JsonConvert.DeserializeObject<ApiFilterfnPaymentRequest>(request.RequestData), identity, log, env);
                case "CancelClientPaymentRequest":
                    return
                        CancelClientPaymentRequest(
                            JsonConvert.DeserializeObject<ChangePaymentRequestState>(request.RequestData), identity, log);
                case "ExportWithdrawalPaymentRequests":// should be removed
                    return
                        ExportWithdrawalPaymentRequests(
                            JsonConvert.DeserializeObject<ApiFilterfnPaymentRequest>(request.RequestData), identity, log, env);
                case "DeleteDepositFromBetShop":
                    return DeleteDepositFromBetShop(JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
                case "GetEntryList":
                    return GetEntryList(JsonConvert.DeserializeObject<PaymentModel>(request.RequestData), identity, log);
                case "UpdatePaymentEntry":
                    return UpdateEntry(JsonConvert.DeserializeObject<ApiUpdatePaymentEntryInput>(request.RequestData), identity, log);
                case "CreatePaymentFormRequest":
                    return CreatePaymentFormRequest(JsonConvert.DeserializeObject<PaymentFormRequest>(request.RequestData), identity, log);
                case "RedirectPaymentRequest":
                    return RedirectPaymentRequest(JsonConvert.DeserializeObject<ApiRedirectPaymentRequestInput>(request.RequestData), identity, log);
                case "CancelDepositFromBetShop":
                    return DeleteDepositFromBetShop(JsonConvert.DeserializeObject<PaymentSystemModel>(request.RequestData).Id, identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        public static ApiResponseBase GetPaymentSystems(SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var result = paymentSystemBl.GetPaymentSystems();
                return new ApiResponseBase
                {
                    ResponseObject =
                        result.Select(
                            x => x.MapToPaymentSystemModel(identity.TimeZone))
                };
            }
        }

        private static ApiResponseBase GetPartnerPaymentSettings(ApiFilterfnPartnerPaymentSetting filter, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var paymentSettings =
                    paymentSystemBl.GetfnPartnerPaymentSettings(filter.MapToFilterfnPartnerPaymentSetting(), true);
                var response = new ApiResponseBase
                {
                    ResponseObject = paymentSettings.Select(x => x.MapTofnPartnerPaymentSettingModel(identity.TimeZone)).
                                     OrderBy(x => x.State).ThenBy(x => x.Type).ThenByDescending(x => x.Id).ToList().ToList()
                };
                return response;
            }
        }

        private static ApiResponseBase GetPartnerPaymentSettingById(int partnerPaymentSettingId, SessionIdentity identity, ILog log)
        {
            using var paymentSystemBl = new PaymentSystemBll(identity, log);
            var partnerPaymentSetting = paymentSystemBl.GetPartnerPaymentSettingById(partnerPaymentSettingId);
            if (partnerPaymentSetting == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            return new ApiResponseBase
            {
                ResponseObject = partnerPaymentSetting.MapToApiPartnerPaymentSetting(identity.TimeZone)
            };
        }



        private static ApiResponseBase UpdatePartnerPaymentSetting(ApiPartnerPaymentSetting apiPartnerPaymentSetting, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var paymentSystem = paymentSystemBl.SavePartnerPaymentSetting(apiPartnerPaymentSetting.MapToPartnerPaymentSetting());
                var res = paymentSystem.MapToApiPartnerPaymentSetting(identity.TimeZone);
                res.PaymentSystemName = CacheManager.GetPaymentSystemById(res.PaymentSystemId).Name;
                var key = string.Format("{0}_{1}_{2}_{3}_{4}", Constants.CacheItems.PaymentSystems, res.PartnerId, res.PaymentSystemId, res.CurrencyId, res.Type);
                var cacheObj = new BllPartnerPaymentSetting
                {
                    Id = paymentSystem.Id,
                    PartnerId = paymentSystem.PartnerId,
                    PaymentSystemId = paymentSystem.PaymentSystemId,
                    CurrencyId = paymentSystem.CurrencyId,
                    State = paymentSystem.State,
                    SessionId = paymentSystem.SessionId,
                    CreationTime = paymentSystem.CreationTime,
                    LastUpdateTime = paymentSystem.LastUpdateTime,
                    UserName = paymentSystem.UserName,
                    Password = paymentSystem.Password,
                    PaymentSystemPriority = paymentSystem.PaymentSystemPriority,
                    Type = paymentSystem.Type,
                    Commission = paymentSystem.Commission,
                    Info = paymentSystem.Info,
                    MaxAmount = paymentSystem.MaxAmount,
                    MinAmount = paymentSystem.MinAmount,
                    AllowMultipleClientsPerPaymentInfo = paymentSystem.AllowMultipleClientsPerPaymentInfo,
                    AllowMultiplePaymentInfoes = paymentSystem.AllowMultiplePaymentInfoes,
                    OpenMode = paymentSystem.OpenMode,
                    OSTypesString = paymentSystem.OSTypes,
                    Countries = paymentSystem.PartnerPaymentCountrySettings.Select(x => x.CountryId).ToList(),
                    CurrencyRates = paymentSystem.PartnerPaymentCurrencyRates.Select(y => new BllCurrencyRate { Id = y.Id, CurrencyId = y.CurrencyId, Rate = y.Rate }).ToList()
                };
                Helpers.Helpers.InvokeMessage("UpdateCacheItem", key, cacheObj, TimeSpan.FromDays(1d));
                return new ApiResponseBase
                {
                    ResponseObject = res
                };
            }
        }

        private static ApiResponseBase AddPartnerPaymentSetting(ApiPartnerPaymentSetting input, SessionIdentity identity, ILog log)
        {
            using var paymentSystemBl = new PaymentSystemBll(identity, log);
            var setting = CacheManager.GetPartnerPaymentSettings(input.PartnerId, input.PaymentSystemId, input.CurrencyId, input.Type);
            if (setting != null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            var osTypes = Enum.GetValues(typeof(OSTypes)).Cast<int>().ToList();
            if (input.OSTypes != null && input.OSTypes.Any(x => !osTypes.Contains(x)))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            var partnerPaymentSetting = input.MapToPartnerPaymentSetting();
            return new ApiResponseBase
            {
                ResponseObject = paymentSystemBl.SavePartnerPaymentSetting(partnerPaymentSetting).MapToApiPartnerPaymentSetting(identity.TimeZone)
            };
        }

        private static ApiResponseBase SavePartnerPaymentCurrencyRate(ApiPartnerPaymentCurrencyRate input, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var partnerPaymentCurrencyRate = new PartnerPaymentCurrencyRate
                {
                    Id = input.Id,
                    PaymentSettingId = input.PaymentSettingId,
                    CurrencyId = input.CurrencyId,
                    Rate = input.Rate != 0m ? 1 / input.Rate : 0
                };
                var res = paymentSystemBl.SavePartnerPaymentCurrencyRate(partnerPaymentCurrencyRate, out PartnerPaymentSetting partnerPaymentSetting).MapToApiPartnerPaymentCurrencyRate();
                var key = string.Format("{0}_{1}_{2}_{3}_{4}", Constants.CacheItems.PaymentSystems, partnerPaymentSetting.PartnerId, partnerPaymentSetting.PaymentSystemId,
                    partnerPaymentSetting.CurrencyId, partnerPaymentSetting.Type);
                var cacheObj = new BllPartnerPaymentSetting
                {
                    Id = partnerPaymentSetting.Id,
                    PartnerId = partnerPaymentSetting.PartnerId,
                    PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                    CurrencyId = partnerPaymentSetting.CurrencyId,
                    State = partnerPaymentSetting.State,
                    SessionId = partnerPaymentSetting.SessionId,
                    CreationTime = partnerPaymentSetting.CreationTime,
                    LastUpdateTime = partnerPaymentSetting.LastUpdateTime,
                    UserName = partnerPaymentSetting.UserName,
                    Password = partnerPaymentSetting.Password,
                    PaymentSystemPriority = partnerPaymentSetting.PaymentSystemPriority,
                    Type = partnerPaymentSetting.Type,
                    Commission = partnerPaymentSetting.Commission,
                    Info = partnerPaymentSetting.Info,
                    MaxAmount = partnerPaymentSetting.MaxAmount,
                    MinAmount = partnerPaymentSetting.MinAmount,
                    AllowMultipleClientsPerPaymentInfo = partnerPaymentSetting.AllowMultipleClientsPerPaymentInfo,
                    AllowMultiplePaymentInfoes = partnerPaymentSetting.AllowMultiplePaymentInfoes,
                    Countries = partnerPaymentSetting.PartnerPaymentCountrySettings.Select(x => x.CountryId).ToList(),
                    CurrencyRates = partnerPaymentSetting.PartnerPaymentCurrencyRates.Select(y => new BllCurrencyRate { Id = y.Id, CurrencyId = y.CurrencyId, Rate = y.Rate }).ToList()
                };
                Helpers.Helpers.InvokeMessage("UpdateCacheItem", key, cacheObj, TimeSpan.FromDays(1d));
                return new ApiResponseBase
                {
                    ResponseObject = res
                };

            }
        }

        private static ApiResponseBase GetPartnerPaymentCurrencyRates(int partnerPaymentSettingId, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = paymentSystemBl.GetPartnerPaymentCurrencyRates(partnerPaymentSettingId).Select(x => x.MapToApiPartnerPaymentCurrencyRate()).ToList()
                };

            }
        }
        private static ApiResponseBase SavePaymentSystem(PaymentSystemModel paymentSystem, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var response = new ApiResponseBase
                {
                    ResponseObject = paymentSystemBl.SavePaymentSystem(paymentSystem.MapToPaymentSystem()).MapToPaymentSystemModel(identity.TimeZone)
                };
                return response;
            }
        }

        private static ApiResponseBase GetPaymentRequestsPaging(ApiFilterfnPaymentRequest request, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var filter = request.MapToFilterfnPaymentRequest();
                filter.WithPendings = null;
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        PaymentRequests = paymentSystemBl.GetPaymentRequestsPaging(filter, true, true).MapToApiPaymentRequestsReport(identity.TimeZone)
                    }
                };
            }
        }

        public static ApiResponseBase GetPaymentRequestById(long requestId, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var result = paymentSystemBl.GetfnPaymentRequestById(requestId).MapToApiPaymentRequest(identity.TimeZone);
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

        public static ApiResponseBase RejectPaymentRequest(ChangePaymentRequestState request, SessionIdentity identity, ILog log)
        {
            //checkPermission
            using var paymentSystemBl = new PaymentSystemBll(identity, log);
            using var clientBl = new ClientBll(paymentSystemBl);
            using var documentBl = new DocumentBll(paymentSystemBl);
            using var notificationBl = new NotificationBll(paymentSystemBl);
            var r = paymentSystemBl.GetPaymentRequestById(request.PaymentRequestId);
            if (r == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);
            try
            {
                if (r.Type == (int)PaymentRequestTypes.Deposit)
                    clientBl.CancelDeposit(request.PaymentRequestId, request.Comment);
                else
                    clientBl.ChangeWithdrawRequestState(
                        request.PaymentRequestId,
                        PaymentRequestStates.CanceledByUser,
                        request.Comment,
                        request.CashDeskId,
                        null, true, string.Empty, documentBl, notificationBl, request.SendEmail);
            }
            catch { throw; }
            finally
            {
                CacheManager.RemoveClientBalance(r.ClientId);
                CacheManager.RemoveClientDepositCount(r.ClientId);
                Helpers.Helpers.InvokeMessage("ClientDeposit", r.ClientId);
                Helpers.Helpers.InvokeMessage("UpdateCacheItem", string.Format("{0}_{1}", Constants.CacheItems.ClientUnreadTicketsCount, r.ClientId),
                    new BllUnreadTicketsCount { Count = 1 }, TimeSpan.FromHours(6));
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase SetToInProcessPaymentRequest(ChangePaymentRequestState request, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var notificationBl = new NotificationBll(paymentSystemBl))
                    {
                        var r = paymentSystemBl.GetPaymentRequestById(request.PaymentRequestId);
                        if (r.Type == (int)PaymentRequestTypes.Deposit)
                            clientBl.ChangeDepositRequestState(request.PaymentRequestId,
                            PaymentRequestStates.InProcess, request.Comment, notificationBl);
                        else
                            clientBl.ChangeWithdrawPaymentRequestState(request.PaymentRequestId, request.Comment, request.CashDeskId, null, PaymentRequestStates.InProcess);
                        return new ApiResponseBase();
                    }
                }
            }
        }

        private static ApiResponseBase SetToFrozenPaymentRequest(ChangePaymentRequestState request, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var notificationBl = new NotificationBll(paymentSystemBl))
                    {
                        var r = paymentSystemBl.GetPaymentRequestById(request.PaymentRequestId);
                        if (r.Type == (int)PaymentRequestTypes.Deposit)
                            clientBl.ChangeDepositRequestState(request.PaymentRequestId,
                            PaymentRequestStates.Frozen, request.Comment, notificationBl);
                        else
                            clientBl.ChangeWithdrawPaymentRequestState(request.PaymentRequestId, request.Comment, request.CashDeskId, null, PaymentRequestStates.Frozen);
                        return new ApiResponseBase();
                    }
                }
            }
        }

        private static ApiResponseBase SetToWaitingForKYCPaymentRequest(ChangePaymentRequestState request, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var notificationBl = new NotificationBll(paymentSystemBl))
                    {
                        var r = paymentSystemBl.GetPaymentRequestById(request.PaymentRequestId);
                        if (r.Type == (int)PaymentRequestTypes.Deposit)
                            clientBl.ChangeDepositRequestState(request.PaymentRequestId,
                            PaymentRequestStates.WaitingForKYC, request.Comment, notificationBl);
                        else
                            clientBl.ChangeWithdrawPaymentRequestState(request.PaymentRequestId, request.Comment, request.CashDeskId, null, PaymentRequestStates.WaitingForKYC);
                        return new ApiResponseBase();
                    }
                }
            }
        }

        private static ApiResponseBase AllowPaymentRequest(ChangePaymentRequestState request, SessionIdentity identity, ILog log)
        {
            using var paymentSystemBl = new PaymentSystemBll(identity, log);
            using var clientBl = new ClientBll(paymentSystemBl);
            using var documentBl = new DocumentBll(paymentSystemBl);
            using var notificationBl = new NotificationBll(paymentSystemBl);
            var r = paymentSystemBl.GetPaymentRequestById(request.PaymentRequestId);
            if (r.Type == (int)PaymentRequestTypes.Deposit)
                clientBl.ChangeDepositRequestState(request.PaymentRequestId,
                PaymentRequestStates.Confirmed, request.Comment, notificationBl);
            else
            {
                var resp = clientBl.ChangeWithdrawRequestState(request.PaymentRequestId, PaymentRequestStates.Confirmed,
                    request.Comment, request.CashDeskId, null, true, r.Parameters, documentBl, notificationBl, request.SendEmail);
                var client = CacheManager.GetClientById(r.ClientId);
                if (r.PaymentSystemId == Constants.BetShopId && client.IsMobileNumberVerified)
                {
                    notificationBl.SendNotificationMessage(new NotificationModel
                    {
                        PartnerId = client.PartnerId,
                        ClientId = client.Id,
                        MobileOrEmail =client.MobileNumber,
                        ClientInfoType = (int)ClientInfoTypes.ConfirmWithdrawSMS,
                        Parameters = string.Format("betshop:{0},cashcode:{1},amount:{2},betshopaddress:{3}",
                        resp.BetShop, resp.CashCode, Math.Floor((resp.RequestAmount - resp.CommissionAmount) * 100) / 100, resp.BetShopAddress),
                        LanguageId = client.LanguageId
                    });
                }
            }
            return new ApiResponseBase();
        }


        private static ApiResponseBase PayPaymentRequest(ChangePaymentRequestState request, SessionIdentity identity, ILog log)
        {
            using var paymentSystemBl = new PaymentSystemBll(identity, log);
            using var clientBl = new ClientBll(paymentSystemBl);
            using var documentBl = new DocumentBll(paymentSystemBl);
            using var notificationBl = new NotificationBll(paymentSystemBl);
            var r = paymentSystemBl.GetPaymentRequestById(request.PaymentRequestId);
            if (r == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);

            if (r.Type == (int)PaymentRequestTypes.Deposit)
            {
                if (r.BetShopId != null)
                    clientBl.ApproveDepositFromBetShop(paymentSystemBl, request.PaymentRequestId, request.Comment, documentBl, notificationBl);
                else
                    clientBl.ApproveDepositFromPaymentSystem(r, true, request.Comment);

                Helpers.Helpers.InvokeMessage("ClientDepositWithBonus", r.ClientId);
                return new ApiResponseBase();
            }
            if (r.BetShopId.HasValue)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);

            clientBl.CheckPermission(Constants.Permissions.PayPaymentRequest);

            var response = new PaymentResponse();
            if (r.Status != (int)PaymentRequestStates.PayPanding)
            {
                if (r.Status != (int)PaymentRequestStates.Confirmed)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.CanNotChangePaymentRequestStatus);

                response = PaymentHelpers.SendPaymentWithdrawalsRequest(r, identity, log);
            }
            else
            {
                var paymentSystem = CacheManager.GetPaymentSystemById(r.PaymentSystemId);
                if (paymentSystem.Name != PaymentSystems.PayOne)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.CanNotChangePaymentRequestStatus);

                response.Status = PaymentRequestStates.ApprovedManually;
            }

            if (response.Status == PaymentRequestStates.Approved || response.Status == PaymentRequestStates.ApprovedManually)
            {
                var resp = clientBl.ChangeWithdrawRequestState(request.PaymentRequestId, response.Status, request.Comment, null, null, true, request.Parameters, documentBl, notificationBl, request.SendEmail);
                clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, resp.ClientId));
                return new ApiResponseBase();
            }
            else if (response.Status == PaymentRequestStates.PayPanding)
            {
                clientBl.ChangeWithdrawRequestState(request.PaymentRequestId, PaymentRequestStates.PayPanding, request.Comment, null, null, true, request.Parameters, documentBl, notificationBl, request.SendEmail);
                return new ApiResponseBase
                {
                    ResponseCode = Constants.SuccessResponseCode,
                    Description = response.Description
                };
            }
            else
            {
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = response.Description
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

        private static ApiResponseBase ExportDepositPaymentRequests(ApiFilterfnPaymentRequest request, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var timeZone = paymentSystemBl.GetUserIdentity().TimeZone;
                var filter = request.MapToFilterfnPaymentRequest();
                var result = paymentSystemBl.ExportDepositPaymentRequests(filter).Select(x => x.MapToApiPaymentRequest(timeZone)).ToList();
                string fileName = "ExportDepositPaymentRequests.csv";
                string fileAbsPath = paymentSystemBl.ExportToCSV<ApiPaymentRequest>(fileName, result, request.FromDate, request.ToDate, timeZone, env);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase CancelClientPaymentRequest(ChangePaymentRequestState request, SessionIdentity identity, ILog log)
        {
            using var paymentSystemBl = new PaymentSystemBll(identity, log);
            using var clientBl = new ClientBll(paymentSystemBl);
            using var documentBl = new DocumentBll(paymentSystemBl);
            using var notificationBl = new NotificationBll(paymentSystemBl);
            var r = paymentSystemBl.GetPaymentRequestById(request.PaymentRequestId);
            if (r.Type == (int)PaymentRequestTypes.Deposit)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
            try
            {
                clientBl.ChangeWithdrawRequestState(request.PaymentRequestId, PaymentRequestStates.CanceledByClient, request.Comment,
                     request.CashDeskId, null, true, string.Empty, documentBl, notificationBl, request.SendEmail);
            }
            catch { throw; }
            finally
            {
                CacheManager.RemoveClientBalance(r.ClientId);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, r.ClientId));
            }
            return new ApiResponseBase();
        }
        private static ApiResponseBase ExportWithdrawalPaymentRequests(ApiFilterfnPaymentRequest request, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var timeZone = paymentSystemBl.GetUserIdentity().TimeZone;
                var filter = request.MapToFilterfnPaymentRequest();
                var result = paymentSystemBl.ExportWithdrawalPaymentRequests(filter).Select(x => x.MapToApiPaymentRequest(timeZone)).ToList();
                string fileName = "ExportWithdrawalPaymentRequests.csv";
                string fileAbsPath = paymentSystemBl.ExportToCSV<ApiPaymentRequest>(fileName, result, request.FromDate, request.ToDate, timeZone, env);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase DeleteDepositFromBetShop(int paymentRequestId, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var res = paymentSystemBl.DeleteDepositFromBetShop(paymentRequestId);
                CacheManager.RemoveClientBalance(res[0].ClientId.Value);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, res[0].ClientId.Value));
                return new ApiResponseBase();
            }
        }

        public static ApiResponseBase GetEntryList(PaymentModel paymentModel, SessionIdentity identity, ILog log)
        {
            var response = new ApiResponseBase
            {
                ResponseObject = SerosPayHelpers.GetPaymentEntries(paymentModel.PaymentSystemIds[0], paymentModel.PartnerId, identity, log)
            };
            return response;
        }

        public static ApiResponseBase UpdateEntry(ApiUpdatePaymentEntryInput entryField, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                var request = paymentSystemBl.GetPaymentRequestById(entryField.PaymentRequestId);
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);

                request.ExternalTransactionId = CommonFunctions.GetRandomString(10);
                if (!string.IsNullOrEmpty(partnerPaymentSetting.UserName) || !string.IsNullOrEmpty(partnerPaymentSetting.Password))
                {
                    var entry = SerosPayHelpers.UpdatePaymentEntries(entryField.EntryModel, request, identity, log);
                    request.ExternalTransactionId = entry.Id;
                    request.Amount = Convert.ToDecimal(entry.PaymentAmount);
                }
                else
                    request.ExternalTransactionId = request.Id.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(request);
                using (var clientBl = new ClientBll(identity, log))
                {
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                    Helpers.Helpers.InvokeMessage("ClientDepositWithBonus", request.ClientId);
                }
                var response = new ApiResponseBase();
                return response;
            }
        }

        public static ApiResponseBase CreatePaymentFormRequest(PaymentFormRequest paymentFormRequest, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                BllClient client = null;
                if (int.TryParse(paymentFormRequest.ClientIdentifier, out int clientId))
                    client = CacheManager.GetClientById(clientId);
                else
                    client = CacheManager.GetClientByUserName(paymentFormRequest.PartnerId, paymentFormRequest.ClientIdentifier);
                if (client == null)
                    throw BaseBll.CreateException(identity.LanguageId, Errors.ClientNotFound);

                var paymentSystem = CacheManager.GetPaymentSystemByName(PaymentSystems.BankTransferSwift);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, paymentFormRequest.Type);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(identity.LanguageId, Errors.PartnerPaymentSettingNotFound);

                var paymentRequest = new PaymentRequest
                {
                    Amount = paymentFormRequest.Amount,
                    ClientId = client.Id,
                    CurrencyId = client.CurrencyId,
                    Info = paymentFormRequest.Info,
                    PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                    PartnerPaymentSettingId = partnerPaymentSetting.Id,
                    Type = paymentFormRequest.Type
                };
                var result = new PaymentRequest();

                if (paymentFormRequest.Type == (int)PaymentRequestTypes.Deposit)
                    result = clientBl.UploadPaymentForm(paymentRequest, paymentFormRequest.PaymentForm, paymentFormRequest.ImageName);
                else if (paymentFormRequest.Type == (int)PaymentRequestTypes.Withdraw)
                    result = clientBl.UploadPaymentForm(paymentRequest, string.Empty, string.Empty);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, client.Id));
                Helpers.Helpers.InvokeMessage("PaymentRequst", result.Id);
                using (var paymentSystemBl = new PaymentSystemBll(identity, log))
                {
                    return new ApiResponseBase
                    {
                        ResponseObject = paymentSystemBl.GetfnPaymentRequestById(result.Id).MapToApiPaymentRequest(identity.TimeZone)
                    };
                }
            }
        }

        public static ApiResponseBase RedirectPaymentRequest(ApiRedirectPaymentRequestInput input, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(identity, log))
            {
                return new ApiResponseBase { };
            }
        }
    }
}