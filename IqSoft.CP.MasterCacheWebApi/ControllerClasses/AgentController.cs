using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.Filters;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.WebSiteModels.Bonuses;
using IqSoft.CP.Common.Models.WebSiteModels.Clients;
using IqSoft.CP.Common.Models.WebSiteModels.ComplimentaryPoint;
using IqSoft.CP.Common.Models.WebSiteModels.Filters;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.DAL.Models.Notification;
using IqSoft.CP.Integration.Payments.Helpers;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.MasterCacheWebApi.Helpers;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using static IqSoft.CP.Common.Constants;

namespace IqSoft.CP.MasterCacheWebApi.ControllerClasses
{
    public static class AgentController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetReportByAgents":
                    return GetReportByAgents(JsonConvert.DeserializeObject<ApiFilterfnAgent>(request.RequestData), identity, log);
                case "GetReportByBets":
                    return GetReportByBets(JsonConvert.DeserializeObject<ApiFilterInternetBet>(request.RequestData), identity, log);
                case "GetTransactions":
                    return GetTransactions(JsonConvert.DeserializeObject<ApiFilterfnAgentTransaction>(request.RequestData), identity, log);
                case "GetDownlineClients":
                    return GetDownlineClients(identity, log);
                case "CreateClientDebitCorrection":
                    return CreateDebitCorrection(JsonConvert.DeserializeObject<ClientCorrectionInput>(request.RequestData), identity, log);
                case "CreateClientCreditCorrection":
                    return CreateCreditCorrection(JsonConvert.DeserializeObject<ClientCorrectionInput>(request.RequestData), identity, log);
                case "GetClientCorrections":
                    return GetClientCorrections(JsonConvert.DeserializeObject<ApiFilterClientCorrection>(request.RequestData), identity, log);
                default:
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
            }
        }

        private static ApiResponseBase GetReportByAgents(ApiFilterfnAgent apiFilter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var agentUser = CacheManager.GetUserById(identity.Id);

                var filter = apiFilter.ToFilterfnUser();
                filter.ParentId = identity.Id;
                var result = reportBl.GetAgentsReport(filter, false);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.ToApiAgentReportItem(identity.TimeZone)).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByBets(ApiFilterInternetBet input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                if (client == null || client.Id == 0 || client.UserId != identity.Id)
                    return new ApiResponseBase();
                var filter = input.MaptToFilterWebSiteBet();
                filter.ToDate = filter.ToDate.AddHours(1);
                var bets = reportBl.GetBetsForWebSite(filter);
                var apibets = bets.Entities.Select(x => x.MapToBetModel(identity.TimeZone, identity.LanguageId)).ToList();
                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Bets = apibets,
                        bets.TotalWinAmount,
                        bets.TotalBetAmount,
                        TotalProfit = bets.TotalGGR,
                        bets.TotalPlayersCount,
                        bets.TotalProvidersCount,
                        bets.TotalPossibleWinAmount,
                        bets.TotalProductsCount
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase GetTransactions(ApiFilterfnAgentTransaction apiFilter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                using (var userBl = new UserBll(reportBl))
                {
                    if (identity.IsAffiliate)
                    {
                        var result = reportBl.GetAffiliateTransactions(apiFilter.MapToFilterfnAffiliateTransaction(), identity.Id);

                        return new ApiResponseBase
                        {
                            ResponseObject = new
                            {
                                result.Count,
                                Entities = result.Entities.Select(x => x.MapToApifnAffiliateTransaction(identity.TimeZone)).ToList()
                            }
                        };
                    }
                    else
                    {
                        BllUser user = null;
                        var agentUser = CacheManager.GetUserById(identity.Id);
                        var isAgentEmploye = agentUser.Type == (int)UserTypes.AgentEmployee;
                        if (isAgentEmploye)
                            agentUser = CacheManager.GetUserById(agentUser.ParentId.Value);

                        var filter = apiFilter.MapToFilterfnAgentTransaction();
                        if (!string.IsNullOrEmpty(apiFilter.UserIdentity))
                        {
                            if (int.TryParse(apiFilter.UserIdentity, out int userId))
                                user = CacheManager.GetUserById(userId);
                            else
                                user = CacheManager.GetUserByUserName(identity.PartnerId, apiFilter.UserIdentity);
                            if (user == null)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                            if (!agentUser.Path.Contains(string.Format("/{0}/", user.Id)))
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                            filter.UserId = user.Id;
                        }
                        var result = reportBl.GetAgentTransactions(filter, identity.Id, isAgentEmploye);

                        return new ApiResponseBase
                        {
                            ResponseObject = new
                            {
                                result.Count,
                                Entities = result.Entities.Select(x => x.MapToApifnAgentTransaction(identity.TimeZone)).ToList()
                            }
                        };
                    }
                }
            }
        }

        private static ApiResponseBase GetDownlineClients(SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var resp = clientBl.GetAgentClients(new FilterClientModel(), identity.Id, true, null);

                return new ApiResponseBase
                {
                    ResponseObject = resp.Select(x => new { Id = x.Id, UserName = x.UserName }).OrderByDescending(x => x.Id).ToList()
                };
            }
        }

        private static ApiResponseBase CreateDebitCorrection(ClientCorrectionInput input, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            if (user.Type == (int)UserTypes.AdminUser)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
            using (var clientBl = new ClientBll(identity, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    using (var userBl = new UserBll(clientBl))
                    {
                        var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                        if (isAgentEmploye)
                            userBl.CheckPermission(Constants.Permissions.CreateDebitCorrectionOnClient);
                        var client = CacheManager.GetClientById(input.ClientId);
                        if (client == null || client.UserId != (isAgentEmploye ? user.ParentId : user.Id))
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
                        input.IsFromAgent = true;

                        var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.BetShop);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        if (partnerPaymentSetting != null && partnerPaymentSetting.Id > 0 && partnerPaymentSetting.State != (int)PartnerPaymentSettingStates.Inactive)
                        {
                            var acc = documentBl.GetAccountIfExists(client.Id, (int)ObjectTypes.Client, client.CurrencyId,
                                (int)AccountTypes.ClientUnusedBalance, null, paymentSystem.Id);
                            if (acc != null)
                            {
                                if (input.AccountId != null && input.AccountId != acc.Id)
                                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                                if (input.AccountTypeId != null && input.AccountTypeId != (int)AccountTypes.ClientUnusedBalance)
                                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                                input.AccountId = acc.Id;
                            }
                        }

                        var result = clientBl.CreateDebitCorrectionOnClient(input, documentBl, false);
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, client.Id));
                        return new ApiResponseBase
                        {
                            ResponseObject = result.ToDocumentModel(identity.TimeZone)
                        };
                    }
                }
            }
        }

        private static ApiResponseBase CreateCreditCorrection(ClientCorrectionInput input, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            if (user.Type == (int)UserTypes.AdminUser)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
            using (var clientBl = new ClientBll(identity, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    using (var userBl = new UserBll(clientBl))
                    {
                        var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                        if (isAgentEmploye)
                            userBl.CheckPermission(Constants.Permissions.CreateCreditCorrectionOnClient);
                        var client = CacheManager.GetClientById(input.ClientId);
                        if (client == null || client.UserId != (isAgentEmploye ? user.ParentId : user.Id))
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
                        input.IsFromAgent = true;

                        var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.BetShop);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        if (partnerPaymentSetting != null && partnerPaymentSetting.Id > 0 && partnerPaymentSetting.State != (int)PartnerPaymentSettingStates.Inactive)
                        {
                            var acc = documentBl.GetAccountIfExists(client.Id, (int)ObjectTypes.Client, client.CurrencyId,
                                (int)AccountTypes.ClientUnusedBalance, null, paymentSystem.Id);
                            if (acc != null)
                            {
                                if (input.AccountId != null && input.AccountId != acc.Id)
                                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                                if (input.AccountTypeId != null && input.AccountTypeId != (int)AccountTypes.ClientUnusedBalance)
                                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                                input.AccountId = acc.Id;
                            }
                        }

                        var result = clientBl.CreateCreditCorrectionOnClient(input, documentBl, false);

                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, client.Id));
                        return new ApiResponseBase
                        {
                            ResponseObject = result.ToDocumentModel(identity.TimeZone)
                        };
                    }
                }
            }
        }

        public static ApiResponseBase GetClientCorrections(ApiFilterClientCorrection filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                using (var userBl = new UserBll(reportBl))
                {
                    var user = userBl.GetUserById(identity.Id);
                    if (user == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    var corrections = reportBl.GetClientCorrections(filter.MapToFilterCorrection(), false);

                    return new ApiResponseBase
                    {
                        ResponseObject = corrections.ToApiClientCorrections(identity.TimeZone)
                    };
                }
            }
        }
    }
}