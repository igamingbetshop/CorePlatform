using System;
using System.Linq;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using System.Collections.Generic;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Helpers;
using log4net;
using System.ServiceModel;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.WebSiteModels.Filters;
using IqSoft.CP.Common.Models.WebSiteModels.Bonuses;
using IqSoft.CP.Common.Models.WebSiteModels.Bets;
using IqSoft.CP.MasterCacheWebApi.Helpers;
using IqSoft.CP.Integration.Platforms.Helpers;
using static IqSoft.CP.Common.Constants;
using System.Web;
using System.IO;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.MasterCacheWebApi.ControllerClasses
{
    public static class DocumentController
    {
        public static GetBalanceOutput GetClientBalance(int clientId)
        {
            var client = CacheManager.GetClientById(clientId);
            var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
            if (isExternalPlatformClient)
            {
                var externalBalance = Math.Floor(ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), clientId) * 100) / 100;
                return new GetBalanceOutput
                {
                    AvailableBalance = externalBalance,
                    CurrencyId = client.CurrencyId,
                    Balances = new List<ApiAccountBalance>{ new ApiAccountBalance
                    {
                        TypeId = (int)AccountTypes.ClientUnusedBalance,
                        Balance = externalBalance
                    }}
                };
            }
            var clientBalance = CacheManager.GetClientCurrentBalance(clientId);
            return new GetBalanceOutput
            {
                AvailableBalance = Math.Floor(clientBalance.Balances.Where(x => x.TypeId != (int)AccountTypes.ClientBonusBalance &&
                                                                                x.TypeId != (int)AccountTypes.ClientCompBalance &&
                                                                                x.TypeId != (int)AccountTypes.ClientCoinBalance)
                                                                    .Sum(x => x.Balance) * 100) / 100,
                CurrencyId = clientBalance.CurrencyId,
                Balances = clientBalance.Balances.Select(x => new ApiAccountBalance
                {
                    Id = x.Id,
                    TypeId = x.TypeId,
                    Balance = (x.TypeId == (int)AccountTypes.ClientCoinBalance || x.TypeId == (int)AccountTypes.ClientCompBalance) ? Math.Truncate(x.Balance) : x.Balance,
                    BetShopId = x.BetShopId,
                    PaymentSystemId = x.PaymentSystemId
                })
            };
        }

        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity session, ILog log)
        {
            switch (request.Method)
            {
                case "CreatePaymentRequest": 
                    return CreatePaymentRequest(JsonConvert.DeserializeObject<PaymentRequestModel>(request.RequestData),
                                                request.ClientId,session, log);
                case "CreateDepositRequest":
                    return CreateDepositRequest(JsonConvert.DeserializeObject<CreateDepositRequestInput>(request.RequestData),
                                                request.PartnerId, request.ClientId, session, log);
                case "GetPaymentRequestComments":
                    return GetPaymentRequestComments(Convert.ToInt64(request.RequestData), session, log);
                case "GetPaymentBanks":
                    return GetPaymentBanks(Convert.ToInt32(request.RequestData), request.PartnerId, request.ClientId, session, log);
                case "GetPaymentRequests":
                    return GetPaymentRequests(JsonConvert.DeserializeObject<ApiFilterPaymentRequest>(request.RequestData), request.ClientId, session, log);
                case "GetPendingPaymentRequests":
                    return GetPendingPaymentRequests(JsonConvert.DeserializeObject<ApiFilterPaymentRequest>(request.RequestData), request.ClientId, session, log);
                case "GetBetInfo":
                    session.PartnerId = request.PartnerId;
                    return GetBetInfo(Convert.ToInt64(request.RequestData), request.ProductId, session, log);
                case "GetBetsHistory":
                    return GetBetsHistory(JsonConvert.DeserializeObject<ApiFilterInternetBet>(request.RequestData), request.PartnerId, request.ClientId, session, log);
                case "GetTransactionHistory": 
                    return GetTransactionHistory(JsonConvert.DeserializeObject<ApiFilterTransaction>(request.RequestData), request.ClientId, session, log);
                case "GetBonusBetInfo": 
                    return GetBonusBetInfo(JsonConvert.DeserializeObject<ApiGetBonusBetsInput>(request.RequestData), session);
                case "GetBonusBalance":
                    return GetBonusBalance(request.ClientId, Convert.ToInt32(request.RequestData), session, log);
                case "GetBonuses":
                    return GetBonuses(request.ClientId, JsonConvert.DeserializeObject<ApiGetBonusBetsInput>(request.RequestData), session, log);
                case "CancelClientBonus":
                    return CancelClientBonus(request.ClientId, JsonConvert.DeserializeObject<ApiGetBonusBetsInput>(request.RequestData), session, log);
                case "GetDepositBonusInfo":
                    return GetDepositBonusInfo(request.ClientId, Convert.ToInt32(request.RequestData), session, log);

                case "GetAffiliateClientsOfManager":
                    return GetAffiliateClientsOfManager(request.ClientId, Convert.ToInt32(request.RequestData), session, log);
                case "Transfer":
                    return Transfer(request.ClientId, JsonConvert.DeserializeObject<ApiTransferInput>(request.RequestData), session, log);

                case "GetOperationTypes":
                    return GetOperationTypes(session, log);
                case "GetBetStates":
                    return GetBetStates(session.LanguageId);
                case "GetBetTypes":
                    return GetBetTypes(session, log);
                case "GetPaymentRequestStates": 
                    return GetPaymentRequestStates(session, log);
                case "GetPaymentRequestTypes": 
                    return GetTypesEnumByType(nameof(PaymentRequestTypes), session);
                case "GetPaymentAccountTypes":
                    return GetTypesEnumByType(nameof(PaymentAccountTypes), session);
                case "GetBankAccountTypes": 
                    return GetTypesEnumByType(nameof(BankAccountTypes), session);
                default:
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
            }
        }

        public static ApiResponseBase GetTransactionHistory(ApiFilterTransaction input, int clientId, SessionIdentity session, ILog log)
        {
            input.ClientId = clientId;
            using (var documentBl = new DocumentBll(session, log))
            {
                var filter = input.MaptToFilterFnTransaction();
                filter.ObjectTypeId = (int)ObjectTypes.Client;
                filter.ObjectId = input.ClientId;
                if (session.AccountId != null)
                {
                    var account = documentBl.GetAccount(session.AccountId.Value);
                    if ((account.TypeId == (int)AccountTypes.ClientUnusedBalance || account.TypeId == (int)AccountTypes.ClientUsedBalance) &&
                        account.BetShopId == null && account.PaymentSystemId == null)
                    {
                        var accs = documentBl.GetfnAccounts(new FilterfnAccount
                        {
                            ObjectId = clientId,
                            ObjectTypeId = (int)ObjectTypes.Client
                        });
                        filter.AccountIds = accs.Where(x => (x.TypeId == (int)AccountTypes.ClientUnusedBalance ||
                                x.TypeId == (int)AccountTypes.ClientUsedBalance) && x.BetShopId == null && x.PaymentSystemId == null).Select(x => x.Id).ToList();
                    }
                    else
                        filter.AccountIds = new List<long> { session.AccountId.Value };
                }
                var transactions = documentBl.GetFnTransactions(filter);
                var accounts = CacheManager.GetAccountTypes(session.LanguageId);
                return new ApiResponseBase
                {
                    ResponseObject =  new GetTransactionHistoryOutput
                    {
                        Transactions = transactions.Entities.Select(x => x.MapToTransactionModel(accounts, input.TimeZone)).ToList(),
                        Count = transactions.Count
                    }
                };
            }
        }

        public static ApiResponseBase GetBetsHistory(ApiFilterInternetBet input, int partnerId, int clientId, SessionIdentity session, ILog log)
        {
            input.ClientId = clientId;
            input.PartnerId = partnerId;
            using (var reportBl = new ReportBll(session, log))
            {
                var filter = input.MaptToFilterWebSiteBet();
                filter.ToDate = filter.ToDate.AddHours(1);
                if (session.AccountId != null)
                {
                    var account = reportBl.GetAccount(session.AccountId.Value);
                    if ((account.TypeId == (int)AccountTypes.ClientUnusedBalance || account.TypeId == (int)AccountTypes.ClientUsedBalance) &&
                        account.BetShopId == null && account.PaymentSystemId == null)
                    {
                        var accounts = reportBl.GetfnAccounts(new FilterfnAccount
                        {
                            ObjectId = clientId,
                            ObjectTypeId = (int)ObjectTypes.Client
                        });
                        filter.AccountIds = accounts.Where(x => 
                            (x.TypeId == (int)AccountTypes.ClientUnusedBalance || x.TypeId == (int)AccountTypes.ClientUsedBalance) && 
                            x.BetShopId == null && x.PaymentSystemId == null).Select(x => x.Id).ToList();
                    }
                    else
                        filter.AccountIds = new List<long> { session.AccountId.Value };
                }

                
                var bets = reportBl.GetBetsForWebSite(filter);
                
                return new ApiResponseBase
                {
                    ResponseObject = new GetBetsHistoryOutput
                    {
                        Bets = bets.Entities.Select(x => x.MapToBetModel(input.TimeZone, session.LanguageId)).ToList(),
                        Count = bets.Count
                    }
                };
            }
        }

        public static ApiResponseBase GetOperationTypes(SessionIdentity session, ILog log)
        {
            using (var documentBl = new DocumentBll(session, log))
            {
                var partnerConfig = CacheManager.GetConfigParameters(session.PartnerId, Constants.PartnerKeys.ClientOperationTypes);
                var operationTypes = documentBl.GetOperationTypes().Where(x => partnerConfig.Any(y => y.Value == x.Id.ToString())).OrderBy(x => x.Name).ToList();
                return new ApiResponseBase
                {
                    ResponseObject = new GetOperationTypesOutput
                    {
                        OperationTypes = operationTypes.MapToOperationTypeModels()
                    }
                };
            }
        }

        public static CancelPaymentRequestOutput CancelPaymentRequest(CancelPaymentRequestInput input, int clientId,
            SessionIdentity session, ILog log)
        {
            input.ClientId = clientId;
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var documentBl = new DocumentBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(documentBl))
                        {
                            var request = paymentSystemBl.GetPaymentRequestById(input.RequestId);
                            if (request == null || request.Type != (int)PaymentRequestTypes.Withdraw)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);
                            if (request.ClientId != input.ClientId)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongClientId);

                            var paymentSystem = CacheManager.GetPaymentSystemById(request.PaymentSystemId);
                            var integrationType = PaymentHelpers.GetPaymentSystemIntegrationType(paymentSystem);
                            switch (integrationType)
                            {
                                case PayoutCancelationTypes.ExternallyWithCallback:
                                    clientBl.ChangeWithdrawRequestState(input.RequestId, PaymentRequestStates.CancelPending, string.Empty, null, null, false, 
                                    request.Parameters, documentBl, notificationBl);
                                    PaymentHelpers.CancelPayoutRequest(paymentSystem, request, session, log);
                                    break;
                                default:
                                    clientBl.ChangeWithdrawRequestState(input.RequestId, PaymentRequestStates.CanceledByClient, string.Empty, null, null, false, 
                                                                request.Parameters, documentBl, notificationBl);
                                    break;
                            }

                            CacheManager.RemoveClientBalance(request.ClientId.Value);
                            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, request.ClientId));
                            return new CancelPaymentRequestOutput
                            {
                                RequestAmount = request.Amount,
                                PaymentSystemId = request.PaymentSystemId,
                                ApiBalance = CacheManager.GetClientCurrentBalance(request.ClientId.Value).ToApiBalance()
                            };
                        }
                    }
                }
            }
        }

        public static ApiResponseBase GetBetStates(string languageId)
        {
            var objects = BaseBll.GetEnumerations(Constants.EnumerationTypes.DocumentStates, languageId).Where(x => Constants.ClientBetTypes.Contains(x.Value)).ToList();
            return new ApiResponseBase
            {
                ResponseObject = objects.Select(x => x.MapToApiEnumeration()).ToList()
            };
        }

        private static ApiResponseBase GetBetTypes(SessionIdentity session, ILog log)
        {
            var objects = BaseBll.GetEnumerations(Constants.EnumerationTypes.CreditDocumentTypes, session.LanguageId);
            return new ApiResponseBase
            {
                ResponseObject = objects.Select(x => x.MapToApiEnumeration()).ToList()
            };
        }

        private static ApiResponseBase GetPaymentRequestStates(SessionIdentity session, ILog log)
        {
            var objects = BaseBll.GetEnumerations(Constants.EnumerationTypes.PaymentRequestStates, session.LanguageId);
            return new ApiResponseBase
            {
                ResponseObject = objects.Select(x => x.MapToApiEnumeration()).ToList()
            };
        }

        private static ApiResponseBase CreatePaymentRequest(PaymentRequestModel input, int clientId, SessionIdentity session, ILog log)
        {
            if (input.Amount <= 0)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongOperationAmount);
            input.ClientId = clientId;
            using (var clientBl = new ClientBll(session, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    using (var notificationBl = new NotificationBll(clientBl))
                    {
                        var client = CacheManager.GetClientById(clientId);
                        var verificationCodeForWithdraw = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.VerificationCodeForWithdraw);
                        if (verificationCodeForWithdraw != null && verificationCodeForWithdraw.Id != 0 &&
                            int.TryParse(verificationCodeForWithdraw.StringValue, out int clientInfoTypeId))
                        {
                            if (!Enum.IsDefined(typeof(VerificationCodeTypes), clientInfoTypeId))
                                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerKeyNotFound);
                            if (!((VerificationCodeTypes)clientInfoTypeId).ToString().ToLower().Contains("email"))
                                clientBl.VerifyClientMobileNumber(input.VerificationCode, client.MobileNumber, null, client.PartnerId, false, null,
                                    (int)VerificationCodeTypes.WithdrawByMobile, false);
                            else
                                clientBl.VerifyClientEmail(input.VerificationCode, client.Email, null, client.PartnerId, false, null,
                                    (int)VerificationCodeTypes.WithdrawByEmail, false);
                        }
                        input.CurrencyId = client.CurrencyId;
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                            client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                        if (partnerPaymentSetting == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                        if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);
                        if (client.Citizenship.HasValue && partnerPaymentSetting.Countries.Any() && !partnerPaymentSetting.Countries.Contains(client.Citizenship.Value))
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);
                        var partner = CacheManager.GetPartnerById(client.PartnerId);

                        var clientSetting = CacheManager.GetClientSettingByName(clientId, Constants.ClientSettings.UnusedAmountWithdrawPercent);
                        var uawp = partner.UnusedAmountWithdrawPercent;

                        if (clientSetting != null && clientSetting.Id > 0 && clientSetting.NumericValue != null)
                            uawp = clientSetting.NumericValue.Value;

                        var request = clientBl.CreateWithdrawPaymentRequest(input, uawp, client, documentBl, notificationBl);
                        clientBl.CancelWithdrawRelatedCampaigns();
                        var initializeUrl = string.Empty;
                        try
                        {
                            initializeUrl = PaymentHelpers.InitializeWithdrawalsRequest(request, session, log);
                        }
                        catch (FaultException<BllFnErrorType> exc)
                        {
                            WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(exc.Detail));
                            clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed,
                                                                exc.Detail != null ? exc.Detail.Message : exc.Message, null, null, false,
                                                                string.Empty, documentBl, notificationBl);
                            throw;
                        }
                        catch (Exception exc)
                        {
                            WebApiApplication.DbLogger.Error(exc);
                            clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed,
                                                                exc.Message, null, null, false, string.Empty, documentBl, notificationBl);
                            throw;
                        }
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ActiveBonusId, client.Id));
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, client.Id));
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, client.Id));
                        Helpers.Helpers.InvokeMessage("PaymentRequst", request.Id);
                        var paymentSystem = CacheManager.GetPaymentSystemById(request.PaymentSystemId);

                        var partnerAutoConfirmWithdrawMaxAmount = BaseBll.ConvertCurrency(partner.CurrencyId, input.CurrencyId, partner.AutoConfirmWithdrawMaxAmount);
                        var requireDocument = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.RequireDocumentForWithdrawal);
                        if ((partnerAutoConfirmWithdrawMaxAmount > request.Amount && (client.IsDocumentVerified || requireDocument != "1")) || client.Email == Constants.CardReaderClientEmail ||
                            paymentSystem.Name.ToLower() == Constants.PaymentSystems.IqWallet.ToLower())
                        {
                            clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Confirmed, "", request.CashDeskId, null, false, string.Empty, documentBl, notificationBl);
                        }
                        var response = request.MapToPaymentRequestModel();
                        response.Url = initializeUrl;
                        response.ApiBalance = CacheManager.GetClientCurrentBalance(clientId).ToApiBalance();
                        return new ApiResponseBase
                        {
                            ResponseObject = response
                        };
                    }
                }
            }
        }

        public static ApiResponseBase GetPaymentRequests(ApiFilterPaymentRequest input, int clientId, SessionIdentity session, ILog log)
        {
            input.ClientId = clientId;
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(session, log))
                {
                    var filter = input.MapToFilterPaymentRequest();
                    if (session.AccountId != null)
                    {
                        var account = paymentSystemBl.GetAccount(session.AccountId.Value);
                        if (account.PaymentSystemId.HasValue || account.BetShopId.HasValue)
                            filter.AccountIds = new List<long> { session.AccountId.Value };
                        else
                        {
                            var accounts = clientBl.GetClientAccounts(clientId, false);
                            filter.AccountIds = accounts.Where(x => x.BetShopId == null && x.PaymentSystemId == null &&
                                (x.TypeId == (int)AccountTypes.ClientUnusedBalance || x.TypeId == (int)AccountTypes.ClientUsedBalance)).Select(x => x.Id).ToList();
                        }
                    }
                    var requests = paymentSystemBl.GetPaymentRequestsPaging(filter, false, false);

                    var canceledRequestIds = requests.Entities.Where(x => x.Status == (int)PaymentRequestStates.CanceledByUser).Select(x => x.Id).ToList();
                    var historyItems = paymentSystemBl.GetPaymentRequestHistories(canceledRequestIds, (int)PaymentRequestStates.CanceledByUser);
                    var requestStateNames = BaseBll.GetEnumerations(Constants.EnumerationTypes.PaymentRequestStates, session.LanguageId);
                    return new ApiResponseBase
                    {
                        ResponseObject = new GetPaymentRequestsOutput
                        {
                            PaymentRequests = requests.Entities.MapTofnPaymentRequestModels(input.TimeZone, requestStateNames, historyItems),
                            Count = requests.Count
                        }
                    };
                }
            }
        }

        public static ApiResponseBase GetPendingPaymentRequests(ApiFilterPaymentRequest input, int clientId, SessionIdentity session, ILog log)
        {
            input.ClientId = clientId;
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var filter = input.MapToFilterPaymentRequest();
                filter.States = new FiltersOperation
                {
                    IsAnd = true,
                    OperationTypeList = new List<FiltersOperationType>
                    {
                        new FiltersOperationType
                        {
                            OperationTypeId = (int)FilterOperations.IsLessThenOrEqualTo,
                            IntValue = (int) PaymentRequestStates.WaitingForKYC
                        },
                        new FiltersOperationType
                        {
                            OperationTypeId = (int)FilterOperations.IsNotEqualTo,
                            IntValue = (int) PaymentRequestStates.CanceledByClient
                        }
                    }
                };
                var requests = paymentSystemBl.GetPaymentRequestsPaging(filter, false, false);
                var requestStateNames = BaseBll.GetEnumerations(Constants.EnumerationTypes.PaymentRequestStates, session.LanguageId);
                return new ApiResponseBase
                {
                    ResponseObject = new GetPaymentRequestsOutput
                    {
                        PaymentRequests = requests.Entities.MapTofnPaymentRequestModels(input.TimeZone, requestStateNames, new List<DAL.Models.Report.PaymentRequestHistoryElement>()),
                        Count = requests.Count
                    }
                };
            }
        }

        private static ApiResponseBase CreateDepositRequest(CreateDepositRequestInput input, int partnerId, int clientId, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(clientId);
            input.CurrencyId = client.CurrencyId;
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, input.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
            if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);
            if (client.Citizenship.HasValue && partnerPaymentSetting.Countries.Any() && !partnerPaymentSetting.Countries.Contains(client.Citizenship.Value))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);

            if (ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.NotAllowed);

            var paymentSystem = CacheManager.GetPaymentSystemById(partnerPaymentSetting.PaymentSystemId);

            if (input.Amount < 0 || (input.Amount == 0 && !VoucherPaymentSystems.Contains(paymentSystem.Name)))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongOperationAmount);

            input.ClientId = clientId;
            input.PartnerId = partnerId;
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var bonusService = new BonusService(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = new PaymentRequest
                            {
                                Amount = input.Amount,
                                ClientId = input.ClientId,
                                CurrencyId = client.CurrencyId,
                                Info = input.Info,
                                PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                                PartnerPaymentSettingId = partnerPaymentSetting.Id,
                                ActivatedBonusType = input.BonusId,
                                PaymentSystemName = paymentSystem.Name,
                                AccountId = session.AccountId,
                                BonusRefused = input.BonusRefused
                            };
                            if (!string.IsNullOrEmpty(input.PaymentForm))
                            {
                                var currentPath = HttpContext.Current.Server.MapPath("~");
                                var parentPath = Path.GetDirectoryName(currentPath);
                                string[] paths = { Path.GetDirectoryName(parentPath), "AdminWebApi", "ClientPaymentForms" };
                                var localPath = Path.Combine(paths);
                                var imgName = CommonFunctions.UploadImage(input.ClientId, input.PaymentForm, input.ImageName, localPath, log);
                                var dic = new Dictionary<string, string> { { "PaymentForm", imgName } };
                                paymentRequest.Parameters = JsonConvert.SerializeObject(dic);
                            }
                            if (VoucherPaymentSystems.Contains(paymentSystem.Name))
                            {
                                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                                var dic = new Dictionary<string, string> { { "VoucherNumber", paymentInfo.VoucherNumber }, { "ActivationCode", paymentInfo.ActivationCode } };
                                paymentRequest.Parameters = JsonConvert.SerializeObject(dic);
                            }

                            clientBl.CreateDepositFromPaymentSystem(paymentRequest, out LimitInfo info);
                            CacheManager.RemoveTotalDepositAmount(paymentRequest.ClientId.Value);
                            Helpers.Helpers.InvokeMessage("RemoveTotalDepositAmount", paymentRequest.ClientId.Value);
                            Helpers.Helpers.InvokeMessage("PaymentRequst", paymentRequest.Id);
                            try
                            {
                                var response = PaymentHelpers.SendPaymentDepositRequest(paymentRequest, partnerId, input.GoBackUrl, input.ErrorPageUrl, session, log);
                                if (VoucherPaymentSystems.Contains(paymentSystem.Name))
                                    Helpers.Helpers.InvokeMessage("ClientDepositWithBonus", paymentRequest.ClientId);

                                return new ApiResponseBase
                                {
                                    ResponseCode = response.Status == PaymentRequestStates.Failed ? Errors.GeneralException : Constants.SuccessResponseCode,
                                    Description = response.Description,
                                    ResponseObject = new
                                    {
                                        Id = paymentRequest.Id,
                                        Url = response.Url,
                                        CancelUrl = response.CancelUrl,
                                        Type = response.Type,
                                        Description = response.Description,
                                        Status = response.Status,
                                        Data = response.Data,
                                        PaymentSystemId = input.PaymentSystemId,
                                        LimitInfo = info,
                                        ApiBalance = CacheManager.GetClientCurrentBalance(clientId).ToApiBalance()
                                    }
                                };
                            }
                            catch (FaultException<BllFnErrorType> exc)
                            {
                                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(exc.Detail));
                                clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, exc.Detail != null ? exc.Detail.Message : exc.Message, notificationBl);
                                throw;
                            }
                            catch (Exception exc)
                            {
                                WebApiApplication.DbLogger.Error(exc);
                                clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, exc.Message, notificationBl);
                                throw;
                            }
                        }
                    }
                }
            }
        }

        public static ApiResponseBase GetBetInfo(long betId, string productId, SessionIdentity session, ILog log)
        {
            using (var documentBl = new DocumentBll(session, log))
            {
                HttpRequestInput requestObject = null;
                BllProduct product = null;
                Document document = null;
                string externalTransactionId = string.Empty;
                int partnerId = 0;
                if (string.IsNullOrEmpty(productId) || productId != Constants.IqSoftSportsbookExternalId.ToString())
                {
                    document = documentBl.GetDocumentById(betId);
                    if (document == null || document.ProductId == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.DocumentNotFound);
                    product = CacheManager.GetProductById(document.ProductId.Value);
                    externalTransactionId = document.ExternalTransactionId;
                    partnerId = document.Client.PartnerId;
                }
                else
                {
                    product = CacheManager.GetProductById(Constants.SportsbookProductId);
                    externalTransactionId = betId.ToString();
                    partnerId = session.PartnerId;
                }
                var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
                var providerName = provider.Name.ToLower();
                if (providerName == Constants.GameProviders.IqSoft.ToLower())
                {
                    var pKey = CacheManager.GetPartnerSettingByKey(partnerId, PartnerKeys.IqSoftBrandId);
                    requestObject = Integration.Products.Helpers.IqSoftHelpers.GetBetInfo(pKey.StringValue, provider, externalTransactionId, 
                        session.LanguageId, product.ExternalId, session.PartnerId);
                }
                else if (providerName == Constants.GameProviders.Internal.ToLower())
                {
                    requestObject = Integration.Products.Helpers.InternalHelpers.GetBetInfo(product, externalTransactionId, 
                        session.LanguageId, product.ExternalId, session.PartnerId);
                }
                else if (providerName == Constants.GameProviders.BAS.ToLower())
                {
                    var pKey = CacheManager.GetGameProviderValueByKey(partnerId, product.GameProviderId ?? 0, Constants.PartnerKeys.WinSystemsTicketUrl); // ??

                    return new ApiResponseBase
                    {
                        ResponseObject = new ApiGetBetInfoOutput
                        {
                            Url = string.Format(pKey, externalTransactionId, product.ExternalId)
                        }
                    };
                }
                if (requestObject != null && requestObject.Url != null)
                {
                    requestObject.RequestHeaders = new Dictionary<string, string>();
                    var resp = CommonFunctions.SendHttpRequest(requestObject, out _);
                    var response = JsonConvert.DeserializeObject<ApiResponseBase>(resp);

                    var info = JsonConvert.DeserializeObject<ApiGetBetInfoOutput>(response.ResponseObject.ToString());
                    info.BetDate = info.BetDate.GetGMTDateFromUTC(session.TimeZone);
                    info.Status = Integration.Products.Helpers.InternalHelpers.GetMappedStatus(info.Status);
                    if (info.BetSelections != null)
                    {
                        foreach (var sel in info.BetSelections)
                        {
                            sel.EventDate = sel.EventDate.GetGMTDateFromUTC(session.TimeZone);
                        }
                    }
                    response.ResponseObject = info;
                    return response;
                }
                else
                {
                    return new ApiResponseBase
                    {
                        ResponseObject = document?.ToApiGetBetInfoOutput()
                    };
                }
            }
        }

        private static ApiResponseBase GetBonusBetInfo(ApiGetBonusBetsInput input, SessionIdentity session)
        {
            HttpRequestInput requestObject = null;
            var product = CacheManager.GetProductById(input.ProductId);
            var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
            switch (provider.Name)
            {
                case Constants.GameProviders.IqSoft:
                    requestObject = Integration.Products.Helpers.InternalHelpers.GetBetInfo(product, input.BetId.ToString(), session.LanguageId, 
                        product.ExternalId, session.PartnerId);
                    break;
                default:
                    break;
            }
            if (requestObject != null && requestObject.Url != null)
            {
                requestObject.RequestHeaders = new Dictionary<string, string>();
                var response = JsonConvert.DeserializeObject<ApiResponseBase>(CommonFunctions.SendHttpRequest(requestObject, out _));
                var info = JsonConvert.DeserializeObject<ApiGetBetInfoOutput>(response.ResponseObject.ToString());
                info.BetDate = info.BetDate.GetGMTDateFromUTC(session.TimeZone);
                foreach (var sel in info.BetSelections)
                {
                    sel.EventDate = sel.EventDate.GetGMTDateFromUTC(session.TimeZone);
                }
                response.ResponseObject = info;
                return response;
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongDocumentId);
        }

        private static ApiResponseBase GetBonusBalance(int clientId, int productId, SessionIdentity session, ILog log)
        {
            using (var documentBl = new DocumentBll(session, log))
            {
                HttpRequestInput requestObject = null;

                var product = CacheManager.GetProductById(productId);
                var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
                var providerName = provider.Name.ToLower();
                if (providerName == Constants.GameProviders.IqSoft.ToLower() || providerName == Constants.GameProviders.Internal.ToLower())
                {
                    requestObject = Integration.Products.Helpers.InternalHelpers.GetBonusBalance(clientId, product, session);
                }
                if (requestObject != null && requestObject.Url != null)
                {
                    requestObject.RequestHeaders = new Dictionary<string, string>();
                    return JsonConvert.DeserializeObject<ApiResponseBase>(CommonFunctions.SendHttpRequest(requestObject, out _));
                }
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongProductId);
            }
        }

        private static ApiResponseBase GetBonuses(int clientId, ApiGetBonusBetsInput input, SessionIdentity session, ILog log)
        {
            switch (input.PlatformId)
            {
                case (int)ExternalPlatformTypes.IQSoft:
                    using (var documentBl = new DocumentBll(session, log))
                    {
                        var resp = documentBl.GetBonuses();
                        var product = CacheManager.GetProductById(input.ProductId);
                        if (product.Id == Constants.PlatformProductId)
                            resp = resp.Where(x => x.Type != (int)BonusTypes.CampaignFreeBet && x.Type != (int)BonusTypes.CampaignWagerSport).ToList();
                        else
                            resp = resp.Where(x => x.Type == (int)BonusTypes.CampaignFreeBet || x.Type == (int)BonusTypes.CampaignWagerSport).ToList();

                        if (input.Status != null && input.Status > 0)
                            resp = resp.Where(x => x.Status == input.Status).ToList();
                        if(input.FromDate != null)
                            resp = resp.Where(x => x.AwardingTime >= input.FromDate).ToList();

                        var bonusStates = BaseBll.GetEnumerations(EnumerationTypes.BonusStates, session.LanguageId).ToDictionary(x => x.Value, x => x.Text);
                        var bonusTypes = BaseBll.GetEnumerations(EnumerationTypes.BonusTypes, session.LanguageId).ToDictionary(x => x.Value, x => x.Text);
                        return new ApiResponseBase
                        {
                            ResponseObject = resp.Select(x =>
                            {
                                var r = x.ToApiClientBonusItem(session.TimeZone);
                                r.StateName = bonusStates.ContainsKey(x.Status) ? bonusStates[x.Status] : string.Empty;
                                r.TypeName = bonusTypes.ContainsKey(x.Type) ? bonusTypes[x.Type] : string.Empty;
                                return r;
                            }).OrderByDescending(x => x.Id).ToList()
                        };
                    }
                case (int)ExternalPlatformTypes.EveryMatrix:
                    var client = CacheManager.GetClientById(clientId);
                    return new ApiResponseBase
                    {
                        ResponseObject = Integration.Products.Helpers.EveryMatrixHelpers.GetPlayerBonuses(client, log)
                    };
                default:
                    return new ApiResponseBase();
            }
        }

        private static ApiResponseBase GetAffiliateClientsOfManager(int managerId, int hours, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = clientBl.GetClientsOfAffiliateManager(managerId, hours)
                };
            }
        }

        private static ApiResponseBase GetPaymentRequestComments(long requestId, SessionIdentity session, ILog log)
        {
            using (var paymentBl = new PaymentSystemBll(session, log))
            {
                var response = string.Empty;
                var items = paymentBl.GetPaymentRequestComments(requestId);
                var item = items.FirstOrDefault(x => x.Status == (int)PaymentRequestStates.CanceledByUser);
                if (item != null)
                    response = item.Comment;
                return new ApiResponseBase
                {
                    ResponseObject = response
                };
            }
        }

        private static ApiResponseBase GetPaymentBanks(int paymentSystemId, int partnerId, int clientId, SessionIdentity session, ILog log)
        {
            using (var paymentBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(clientId);
                if (paymentSystemId > 0)
                {
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, paymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                    if (partnerPaymentSetting == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                }
                var response = paymentBl.GetPartnerBanks(partnerId, (paymentSystemId == 0 ? null : (int?)paymentSystemId), false, (int)BankInfoTypes.BankForCompany, client)
                    .Select(x => x.MapToApiPartnerBankInfo()).ToList();
                return new ApiResponseBase
                {
                    ResponseObject = response
                };
            }
        }

        private static ApiResponseBase Transfer(int clientId, ApiTransferInput transferInput, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var client = CacheManager.GetClientById(clientId);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                    var product = CacheManager.GetProductById(transferInput.ProductId);
                    if (product == null || product.Id == 0 || product.GameProviderId == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

                    var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
                    Document doc = null;
                    switch (provider.Name)
                    {
                        case Constants.GameProviders.GlobalSlots:
                            Integration.Products.Helpers.GlobalSlotsHelpers.TransferToProvider(clientId, identity, log);
                            break;
                        default:
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongProductId);
                    }
                     Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, clientId));
                    return new ApiResponseBase();
                }
            }
        }

        private static ApiResponseBase ApprovedPayBoxMobileRequest(int requestId, string smsCode, SessionIdentity identity, ILog log)
        {
            PayBoxHelpers.ApprovedMobileRequest(smsCode, requestId, identity, log);
            return new ApiResponseBase();
        }

        private static ApiResponseBase CancelClientBonus(int clientId, ApiGetBonusBetsInput input, SessionIdentity identity, ILog log)
        {
            switch (input.PlatformId)
            {
                case (int)ExternalPlatformTypes.IQSoft:
                    using (var clientBl = new ClientBll(identity, log))
                    using (var bonusBl = new BonusService(clientBl))
                    {
                        if (!int.TryParse(input.BonusId, out int clientBonusId))
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
                        var bonusStates = BaseBll.GetEnumerations(EnumerationTypes.BonusStates, identity.LanguageId).ToDictionary(x => x.Value, x => x.Text);
                        var cBonus = clientBl.GetClientBonusById(clientBonusId) ??
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.BonusNotFound);
                        var bonus = bonusBl.GetBonusById(cBonus.BonusId);
                        fnClientBonus clientBonus;
                        if (bonus.Type ==(int)BonusTypes.CampaignFreeSpin)
                        {
                            clientBonus= clientBl.CancelClientFreespin(clientBonusId, true);
                            var bonusProducts = bonus.BonusProducts.Where(x => x.BonusId == bonus.Id && x.ProductId != Constants.PlatformProductId).ToList();
                            if (!bonusProducts.Any())
                                return new ApiResponseBase();
                            bonusProducts.ForEach(bp =>
                            {
                                try
                                {
                                    var product = CacheManager.GetProductById(bp.ProductId);
                                    var gameProvider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
                                    switch (gameProvider.Name)
                                    {
                                        case Constants.GameProviders.TwoWinPower:
                                            Integration.Products.Helpers.TwoWinPowerHelpers.CancelFreeSpin(bonus.PartnerId, clientBonusId);
                                            break;
                                        case Constants.GameProviders.BlueOcean:
                                            Integration.Products.Helpers.BlueOceanHelpers.CancelFreeRound(bonus.PartnerId, clientBonusId.ToString()/*must be bo id*/);
                                            break;
                                        case Constants.GameProviders.PragmaticPlay:
                                            Integration.Products.Helpers.PragmaticPlayHelpers.CancelFreeRound(bonus.PartnerId, bonus.Id);
                                            break;
                                        case Constants.GameProviders.SoftSwiss:
                                            Integration.Products.Helpers.SoftSwissHelpers.CancelFreeRound(bonus.Id, bonus.Id);
                                            break;
                                        case Constants.GameProviders.EveryMatrix:
                                            Integration.Products.Helpers.EveryMatrixHelpers.ForfeitFreeSpinBonus(bonus.Id, bonus.Id, product.Id);
                                            break;
                                        case Constants.GameProviders.PlaynGo:
                                            Integration.Products.Helpers.PlaynGoHelpers.CancelFreeSpinBonus(bonus.Id, bonus.Id, product.Id);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                { log.Error(ex); }
                            });
                        }
                        else
                            clientBonus = clientBl.CancelClientBonus(clientBonusId, false);
                        CacheManager.RemoveClientBalance(clientBonus.ClientId);
                        CacheManager.RemoveClientActiveBonus(clientBonus.ClientId);
                        CacheManager.RemoveClientBonus(clientBonus.ClientId, clientBonus.BonusId);
                        Helpers.Helpers.InvokeMessage("ClientBonus", clientBonus.ClientId);

                        var resp = clientBonus.ToApiClientBonusItem(identity.TimeZone);
                        resp.StateName = bonusStates[clientBonus.Status];

                        return new ApiResponseBase
                        {
                            ResponseObject = resp
                        };
                    }
                case (int)ExternalPlatformTypes.EveryMatrix:
                    var client = CacheManager.GetClientById(clientId);
                    Integration.Products.Helpers.EveryMatrixHelpers.ForfeitBonusWallet(client, input.BonusId, log);
                    return new ApiResponseBase
                    {
                        ResponseObject = Integration.Products.Helpers.EveryMatrixHelpers.GetPlayerBonuses(client, log)
                    };
                default:
                    return new ApiResponseBase();
            }
        }

        private static ApiResponseBase GetDepositBonusInfo(int clientId, int paymentSystemId, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var client = CacheManager.GetClientById(clientId);
                var bonusInfo = clientBl.GetClientDepositBonusInfo(client, paymentSystemId);
                
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                return new ApiResponseBase
                {
                    ResponseObject = bonusInfo.Select(x => new
                    {
                        Id = x.Id,
                        Name = x.Name,
                        BonusTypeId = x.Type,
                        MinAmount = BaseBll.ConvertCurrency(partner.CurrencyId, client.CurrencyId, x.MinAmount ?? 0),
                        MaxAmount = BaseBll.ConvertCurrency(partner.CurrencyId, client.CurrencyId, x.MaxAmount ?? 0),
                        x.HasPromo
                    }).ToList()
                };
            }
        }

        private static ApiResponseBase GetTypesEnumByType(string enumName, SessionIdentity identity)
        {
            return new ApiResponseBase
            {
                ResponseObject = BaseBll.GetEnumerations(enumName, identity.LanguageId).Select(x => x.MapToApiEnumeration()).ToList()
            };
        }
    }
}