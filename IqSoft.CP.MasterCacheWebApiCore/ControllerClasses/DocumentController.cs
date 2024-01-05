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
using System.IO;
using IqSoft.CP.MasterCacheWebApiCore;
using Microsoft.AspNetCore.Hosting;
using IqSoft.CP.Common.Models.CacheModels;

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
                    TypeId = x.TypeId,
                    Balance = (x.TypeId == (int)AccountTypes.ClientCoinBalance || x.TypeId == (int)AccountTypes.ClientCompBalance) ? Math.Truncate(x.Balance) : x.Balance
                })
            };
        }

        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity session, ILog log, IWebHostEnvironment environment)
        {
            switch (request.Method)
            {
                case "CreatePaymentRequest":
                    return CreatePaymentRequest(
                        JsonConvert.DeserializeObject<PaymentRequestModel>(request.RequestData), request.ClientId,
                        session, log);
                case "CreateDepositRequest":
                    return
                        CreateDepositRequest(
                            JsonConvert.DeserializeObject<CreateDepositRequestInput>(request.RequestData),
                            request.PartnerId, request.ClientId, session, log, environment);
                case "GetBetStates":
                    return GetBetStates(session, log);
                case "GetBetTypes":
                    return GetBetTypes(session, log);
                case "GetPaymentRequestStates":
                    return GetPaymentRequestStates(session, log);
                case "GetBetInfo":
                    return GetBetInfo(Convert.ToInt64(request.RequestData), request.ProductId, session, log);
                case "GetBonusBetInfo":
                    return GetBonusBetInfo(JsonConvert.DeserializeObject<ApiGetBonusBetsInput>(request.RequestData), session);
                case "GetBonusBalance":
                    return GetBonusBalance(request.ClientId, Convert.ToInt32(request.RequestData), session, log);
                case "GetBonuses":
                    return GetBonuses(request.ClientId, JsonConvert.DeserializeObject<ApiGetBonusBetsInput>(request.RequestData), session, log);
                case "GetAffiliateClientsOfManager":
                    return GetAffiliateClientsOfManager(request.ClientId, Convert.ToInt32(request.RequestData), session, log);
                case "GetPaymentRequestComments":
                    return GetPaymentRequestComments(Convert.ToInt64(request.RequestData), session, log);
                case "GetPaymentBanks":
                    return GetPaymentBanks(Convert.ToInt32(request.RequestData), request.PartnerId, request.ClientId, session, log);
                case "Transfer":
                    return Transfer(request.ClientId, JsonConvert.DeserializeObject<ApiTransferInput>(request.RequestData), session, log);
                case "SendSMSCode":
                    return SendSMSCode(request.ClientId, request.RequestData, session, log);
                case "CancelClientBonus":
                    return CancelClientBonus(Convert.ToInt32(request.RequestData), session, log);
                case "GetDepositBonusInfo":
                    return GetDepositBonusInfo(request.ClientId, session, log);
                case "GetPaymentRequestTypes":
                    return GetTypesEnumByType(nameof(PaymentRequestTypes), session);
                case "GetBankAccountTypes":
                    return GetTypesEnumByType(nameof(BankAccountTypes), session);

                default:
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
            }
        }

        public static GetTransactionHistoryOutput GetTransactionHistory(ApiFilterTransaction input, int clientId,
            SessionIdentity session, ILog log)
        {
            input.ClientId = clientId;
            using (var documentBl = new DocumentBll(session, log))
            {
                var filter = input.MaptToFilterFnTransaction();
                filter.ObjectTypeId = (int)ObjectTypes.Client;
                filter.ObjectId = input.ClientId;
                var transactions = documentBl.GetFnTransactions(filter);
                var accounts = CacheManager.GetAccountTypes(session.LanguageId);
                return new GetTransactionHistoryOutput
                {
                    Transactions = transactions.Entities.Select(x => x.MapToTransactionModel(accounts, input.TimeZone)).ToList(),
                    Count = transactions.Count
                };
            }
        }

        public static GetBetsHistoryOutput GetBetsHistory(ApiFilterInternetBet input, int partnerId, int clientId,
            SessionIdentity session, ILog log)
        {
            input.ClientId = clientId;
            input.PartnerId = partnerId;
            using (var reportBl = new ReportBll(session, log))
            {
                var filter = input.MaptToFilterWebSiteBet();
                filter.ToDate = filter.ToDate.AddHours(1);
                var bets = reportBl.GetBetsForWebSite(filter);
                return new GetBetsHistoryOutput
                {
                    Bets = bets.Entities.Select(x => x.MapToBetModel(input.TimeZone, session.LanguageId)).ToList(),
                    Count = bets.Count
                };
            }
        }

        public static GetOperationTypesOutput GetOperationTypes(SessionIdentity session, ILog log)
        {
            using (var documentBl = new DocumentBll(session, log))
            {
                var operationTypes = documentBl.GetOperationTypes().Where(x => Constants.ClientOperationTypes.Contains(x.Id)).OrderBy(x => x.Name).ToList();
                return new GetOperationTypesOutput
                {
                    OperationTypes = operationTypes.MapToOperationTypeModels()
                };
            }
        }

        public static CancelPaymentRequestOutput CancelPaymentRequest(CancelPaymentRequestInput input, int clientId, SessionIdentity session, ILog log)
        {
            input.ClientId = clientId;
            using var paymentSystemBl = new PaymentSystemBll(session, log);
            using var clientBl = new ClientBll(paymentSystemBl);
            using var documentBl = new DocumentBll(paymentSystemBl);
            using var notificationBl = new NotificationBll(documentBl);
            var request = paymentSystemBl.GetPaymentRequestById(input.RequestId);
            if (request.ClientId != input.ClientId)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongClientId);
            clientBl.ChangeWithdrawRequestState(input.RequestId, PaymentRequestStates.CanceledByClient, string.Empty, null, null, false, string.Empty, documentBl, notificationBl);
            CacheManager.RemoveClientBalance(request.ClientId);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, request.ClientId));
            return new CancelPaymentRequestOutput
            {
                RequestAmount = request.Amount,
                PaymentSystemId = request.PaymentSystemId,
               // ApiBalance = CacheManager.GetClientCurrentBalance(request.ClientId).ToApiBalance()
            };
        }
        private static ApiResponseBase GetBetStates(SessionIdentity session, ILog log)
        {
            var objects = BaseBll.GetEnumerations(Constants.EnumerationTypes.DocumentStates, session.LanguageId).Where(x => Constants.ClientBetTypes.Contains(x.Value)).ToList();
            var response = new ApiResponseBase
            {
                ResponseObject = objects.Select(x => x.MapToApiEnumeration()).ToList()
            };
            return response;
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
            using var clientBl = new ClientBll(session, log);
            using var documentBl = new DocumentBll(clientBl);
            using var notificationBl = new NotificationBll(clientBl);
            var client = CacheManager.GetClientById(clientId);
            var verificationCodeForWithdraw = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.VerificationCodeForWithdraw);
            if (verificationCodeForWithdraw != null && verificationCodeForWithdraw.Id != 0 &&
                int.TryParse(verificationCodeForWithdraw.StringValue, out int clientInfoTypeId))
            {
                if (!Enum.IsDefined(typeof(VerificationCodeTypes), clientInfoTypeId))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerKeyNotFound);
                if (!((VerificationCodeTypes)clientInfoTypeId).ToString().ToLower().Contains("email"))
                    clientBl.VerifyClientMobileNumber(input.VerificationCode, client.MobileNumber, null, client.PartnerId, false, null, false);
                else
                    clientBl.VerifyClientEmail(input.VerificationCode, client.Email, null, client.PartnerId, false, null, false);
            }
            input.CurrencyId = client.CurrencyId;
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(input.PartnerId, input.PaymentSystemId,
                client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            if (partnerPaymentSetting == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
            if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);
            if (client.Citizenship.HasValue && partnerPaymentSetting.Countries.Any() && !partnerPaymentSetting.Countries.Contains(client.Citizenship.Value))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);
            var partner = CacheManager.GetPartnerById(input.PartnerId);

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
                Program.DbLogger.Error(JsonConvert.SerializeObject(exc.Detail));
                clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed,
                                                    exc.Detail != null ? exc.Detail.Message : exc.Message, null, null, false,
                                                    string.Empty, documentBl, notificationBl);
                throw;
            }
            catch (Exception exc)
            {
                Program.DbLogger.Error(exc);
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
                clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Confirmed, "", request.CashDeskId, null, false, 
                                                    string.Empty, documentBl, notificationBl);
            }
            var response = request.MapToPaymentRequestModel();
            response.Url = initializeUrl;
            return new ApiResponseBase
            {
                ResponseObject = response
            };
        }

        public static GetPaymentRequestsOutput GetPaymentRequests(ApiFilterPaymentRequest input, int clientId,
            SessionIdentity session, ILog log)
        {
            input.ClientId = clientId;
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {

                var filter = input.MapToFilterPaymentRequest();
                var requests = paymentSystemBl.GetPaymentRequestsPaging(filter, false, false);
                var canceledRequestIds = requests.Entities.Where(x => x.Status == (int)PaymentRequestStates.CanceledByUser).Select(x => x.Id).ToList();
                var historyItems = paymentSystemBl.GetPaymentRequestHistories(canceledRequestIds, (int)PaymentRequestStates.CanceledByUser);
                var requestStateNames = BaseBll.GetEnumerations(Constants.EnumerationTypes.PaymentRequestStates, session.LanguageId);
                return new GetPaymentRequestsOutput
                {
                    PaymentRequests = requests.Entities.MapTofnPaymentRequestModels(input.TimeZone, requestStateNames, historyItems),
                    Count = requests.Count
                };
            }
        }

        public static GetPaymentRequestsOutput GetPendingPaymentRequests(ApiFilterPaymentRequest input, int clientId,
            SessionIdentity session, ILog log)
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
                return new GetPaymentRequestsOutput
                {
                    PaymentRequests = requests.Entities.MapTofnPaymentRequestModels(input.TimeZone, requestStateNames, new List<DAL.Models.Report.PaymentRequestHistoryElement>()),
                    Count = requests.Count
                };
            }
        }

        private static ApiResponseBase CreateDepositRequest(CreateDepositRequestInput input, int partnerId, int clientId, 
            SessionIdentity session, ILog log, IWebHostEnvironment env)
        {
            var client = CacheManager.GetClientById(clientId);
            input.CurrencyId = client.CurrencyId;
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
            if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);
            if (client.Citizenship.HasValue && partnerPaymentSetting.Countries.Any() && !partnerPaymentSetting.Countries.Contains(client.Citizenship.Value))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);

            if(ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.NotAllowed);

            var paymentSystem = CacheManager.GetPaymentSystemById(partnerPaymentSetting.PaymentSystemId);

            if (input.Amount < 0 || (input.Amount == 0 && !VoucherPaymentSystems.Contains(paymentSystem.Name)))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongOperationAmount);

            input.ClientId = clientId;
            input.PartnerId = partnerId;
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using var clientBl = new ClientBll(paymentSystemBl);
                using var bonusService = new BonusService(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var paymentRequest = new PaymentRequest
                {
                    Amount = input.Amount,
                    ClientId = input.ClientId,
                    CurrencyId = client.CurrencyId,
                    Info = input.Info,
                    PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                    PartnerPaymentSettingId = partnerPaymentSetting.Id,
                    ActivatedBonusType = input.BonusId,
                    PaymentSystemName = paymentSystem.Name
                };
                if (!string.IsNullOrEmpty(input.PaymentForm))
                {
                    string[] paths = { Path.GetDirectoryName(env.ContentRootPath), "AdminWebApi", "ClientPaymentForms" };
                    var localPath = Path.Combine(paths);
                    var imgName = CommonFunctions.UploadImage(input.ClientId, input.PaymentForm, input.ImageName, localPath);
                    var dic = new Dictionary<string, string>();
                    dic.Add("PaymentForm", imgName);
                    paymentRequest.Parameters = JsonConvert.SerializeObject(dic);
                }
                if (VoucherPaymentSystems.Contains(paymentSystem.Name))
                {
                    var info = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var dic = new Dictionary<string, string>();
                    dic.Add("VoucherNumber", info.VoucherNumber);
                    dic.Add("ActivationCode", info.ActivationCode);
                    paymentRequest.Parameters = JsonConvert.SerializeObject(dic);
                }
                clientBl.CreateDepositFromPaymentSystem(paymentRequest);
                Helpers.Helpers.InvokeMessage("PaymentRequst", paymentRequest.Id);
                try
                {
                    var response = PaymentHelpers.SendPaymentDepositRequest(paymentRequest, partnerId, session, log);
                    if (VoucherPaymentSystems.Contains(paymentSystem.Name))
                        Helpers.Helpers.InvokeMessage("ClientDepositWithBonus", paymentRequest.ClientId);

                    return new ApiResponseBase
                    {
                        ResponseCode = (response.Status == PaymentRequestStates.Failed ? Errors.GeneralException : Constants.SuccessResponseCode),
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
                            PaymentSystemId = input.PaymentSystemId
                        }
                    };
                }
                catch (FaultException<BllFnErrorType> exc)
                {
                    Program.DbLogger.Error(JsonConvert.SerializeObject(exc.Detail));
                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, exc.Detail != null ? exc.Detail.Message : exc.Message, notificationBl);
                    throw;
                }
                catch (Exception exc)
                {
                    Program.DbLogger.Error(exc);
                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, exc.Message, notificationBl);
                    throw;
                }
            }
        }

        public static ApiResponseBase GetBetInfo(long betId, string productId, SessionIdentity session, ILog log)
        {
            using (var documentBl = new DocumentBll(session, log))
            {
                HttpRequestInput requestObject = null;
                BllProduct product = null;
                string externalTransactionId = string.Empty;
                int partnerId = 0;
                if (string.IsNullOrEmpty(productId) || productId != Constants.SportsbookExternalId.ToString())
                {
                    var document = documentBl.GetDocumentById(betId);
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
                    requestObject = Integration.Products.Helpers.IqSoftHelpers.GetBetInfo(pKey.StringValue, provider, externalTransactionId, session.LanguageId, product.ExternalId);
                }
                if (providerName == Constants.GameProviders.Internal.ToLower())
                {
                    requestObject = Integration.Products.Helpers.InternalHelpers.GetBetInfo(product, externalTransactionId, session.LanguageId, product.ExternalId);
                }
                else if (providerName == Constants.GameProviders.WinSystems.ToLower())
                {
                    var pKey = CacheManager.GetGameProviderValueByKey(partnerId, product.GameProviderId ?? 0, Constants.PartnerKeys.WinSystemsTicketUrl);

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
                    foreach (var sel in info.BetSelections)
                    {
                        sel.EventDate = sel.EventDate.GetGMTDateFromUTC(session.TimeZone);
                    }
                    response.ResponseObject = info;
                    return response;
                }
                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongDocumentId);
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
                    requestObject = Integration.Products.Helpers.InternalHelpers.GetBetInfo(product, input.BetId.ToString(), session.LanguageId, product.ExternalId);
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
            using (var documentBl = new DocumentBll(session, log))
            {
                var resp = documentBl.GetBonuses();
                var product = CacheManager.GetProductById(input.ProductId);
                if (product.Id == Constants.PlatformProductId)
                    resp = resp.Where(x => x.BonusType != (int)BonusTypes.CampaignFreeBet && x.BonusType != (int)BonusTypes.CampaignWagerSport).ToList();
                else
                    resp = resp.Where(x => x.BonusType == (int)BonusTypes.CampaignFreeBet || x.BonusType == (int)BonusTypes.CampaignWagerSport).ToList();

                if (input.Status != null && input.Status > 0)
                    resp = resp.Where(x => x.Status == input.Status).ToList();

                var bonusStates = BaseBll.GetEnumerations(EnumerationTypes.BonusStates, session.LanguageId).ToDictionary(x => x.Value, x => x.Text);
                var bonusTypes = BaseBll.GetEnumerations(EnumerationTypes.BonusTypes, session.LanguageId).ToDictionary(x => x.Value, x => x.Text);
                return new ApiResponseBase
                {
                    ResponseObject = resp.Select(x =>
                    {
                        var r = x.ToApiClientBonusItem(session.TimeZone);
                        r.StateName = bonusStates.ContainsKey(x.Status) ? bonusStates[x.Status] : string.Empty;
                        r.TypeName = bonusTypes.ContainsKey(x.BonusType) ? bonusTypes[x.BonusType] : string.Empty;
                        return r;
                    }).OrderByDescending(x => x.Id).ToList()
                };
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
                var response = paymentBl.GetPartnerBanks(partnerId, (paymentSystemId == 0 ? null : (int?)paymentSystemId), false, (int)BankInfoTypes.BankForCompany, client).Select(x => x.MapToApiPartnerBankInfo()).ToList();
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

        private static ApiResponseBase SendSMSCode(int clientId, string mobileNumber, SessionIdentity identity, ILog log)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
            if (!ClientBll.IsMobileNumber(mobileNumber))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.InvalidMobile);
            WooppayHelpers.SendSMSCodeWooppayApi(clientId, mobileNumber, identity, log);
            return new ApiResponseBase();
        }

        private static ApiResponseBase ApprovedPayBoxMobileRequest(int requestId, string smsCode, SessionIdentity identity, ILog log)
        {
            PayBoxHelpers.ApprovedMobileRequest(smsCode, requestId, identity, log);
            return new ApiResponseBase();
        }

        private static ApiResponseBase CancelClientBonus( int bonusId, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var bonusStates = BaseBll.GetEnumerations(EnumerationTypes.BonusStates, identity.LanguageId).ToDictionary(x => x.Value, x => x.Text);

                var clientBonus = clientBl.CancelClientBonus(bonusId, false);
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
        }

        private static ApiResponseBase GetDepositBonusInfo(int clientId, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var client = CacheManager.GetClientById(clientId);
                var bonusInfo = clientBl.GetClientDepositBonusInfo(client);
                
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                return new ApiResponseBase
                {
                    ResponseObject = bonusInfo.Select(x => new
                    {
                        Id = x.Id,
                        Name = x.Name,
                        BonusTypeId = x.BonusType,
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