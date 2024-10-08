using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.AgentWebApi.Filters;
using IqSoft.CP.AgentWebApi.Helpers;
using IqSoft.CP.AgentWebApi.Models;
using log4net;
using Newtonsoft.Json;
using System.Linq;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.Common.Models.Filters;

namespace IqSoft.CP.AgentWebApi.ControllerClasses
{
    public static class ReportController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetTransactions":
                    return GetTransactions(JsonConvert.DeserializeObject<ApiFilterfnAgentTransaction>(request.RequestData), identity, log);
                case "GetReportByAgents":
                    return GetReportByAgents(JsonConvert.DeserializeObject<ApiFilterfnAgent>(request.RequestData), identity, log);
                case "GetReportByAgentInternetBet":
                    return GetReportByAgentInternetBet(JsonConvert.DeserializeObject<ApiFilterInternetBet>(request.RequestData), identity, log);
                case "GetReportByAgentLog":
                    return GetReportByAgentLog(JsonConvert.DeserializeObject<ApiFilterReportByActionLog>(request.RequestData), identity, log);
                case "GetBetInfo":
                    return GetBetInfo(JsonConvert.DeserializeObject<long>(request.RequestData), identity, log);
                case "GetOnlineUsers":
                    return GetOnlineUsers(JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
                case "GetReportByCredit":
                    return GetReportByCredit(identity, log);
                case "GetBetShopBetsReportPaging":
                    return
                        GetBetShopBetsReportPaging(
                            JsonConvert.DeserializeObject<ApiFilterBetShopBet>(request.RequestData), identity, log);
                case "GetReportByProviders":
                    return
                        GetReportByProviders(
                            JsonConvert.DeserializeObject<ApiFilterReportByProvider>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.MethodNotFound);
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

        private static ApiResponseBase GetReportByAgentInternetBet(ApiFilterInternetBet input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                input.AgentId = identity.Id;
                var filter = input.MapToFilterInternetBet();
                filter.FieldNameToOrderBy = "UserPath";
                filter.OrderBy = true;
                var bets = reportBl.GetInternetBetsPagedModel(filter, string.Empty, false);
                var apibets = bets.MapToInternetBetsReportModel(reportBl.GetUserIdentity().TimeZone);

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

        private static ApiResponseBase GetReportByAgents(ApiFilterfnAgent apiFilter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var agentUser = CacheManager.GetUserById(identity.Id);

                var filter = apiFilter.ToFilterfnUser();
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

        private static ApiResponseBase GetReportByAgentLog(ApiFilterReportByActionLog apiFilter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                using (var userBl = new UserBll(reportBl))
                {
                    BllUser user = null;
                    var agentUser = CacheManager.GetUserById(identity.Id);
                    var isAgentEmploye = agentUser.Type == (int)UserTypes.AgentEmployee;
                    if (isAgentEmploye)
                        agentUser = CacheManager.GetUserById(agentUser.ParentId.Value);
                    var filter = apiFilter.MapToFilterReportByActionLog();
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
                    var result = reportBl.GetReportByActionLogPaging(filter, isAgentEmploye);
                    return new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            Entities = result.Entities.MapToApiReportByActionLog(identity.TimeZone),
                            result.Count
                        }
                    };
                }
            }
        }

        public static ApiGetBetInfoResponse GetBetInfo(long betId, SessionIdentity identity, ILog log)
        {
            using (var documentBl = new DocumentBll(identity, log))
            {
                var requestObject = new HttpRequestInput();
                var response = new ApiGetBetInfoResponse();
                var document = documentBl.GetDocumentById(betId);
                if (document == null || document.ProductId == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.DocumentNotFound);
                var childs = documentBl.GetDocumentsByParentId(document.Id);

                var product = CacheManager.GetProductById(document.ProductId.Value);
                var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);

                switch (provider.Name)
                {
                    case GameProviders.IqSoft:
                    case GameProviders.Internal:
                        requestObject = Integration.Products.Helpers.InternalHelpers.GetBetInfo(product, document.ExternalTransactionId, identity.LanguageId, product.ExternalId, identity.PartnerId);
                        if (requestObject != null)
                            response = JsonConvert.DeserializeObject<ApiGetBetInfoResponse>(CommonFunctions.SendHttpRequest(requestObject, out _));

                        response.Documents = childs.Select(x => new ApiBetInfoItem
                        {
                            Id = x.Id,
                            OperationTypeId = x.OperationTypeId,
                            CreationTime = x.CreationTime.GetGMTDateFromUTC(identity.TimeZone),
                            RoundId = x.RoundId
                        }).ToList();
                        break;
                    case GameProviders.Ezugi:
                        var ezugiResult = Integration.Products.Helpers.EzugiHelpers.GetReport(document.RoundId.Split('-')[0], document.CreationTime);
                        if (ezugiResult.RoundResult.Any())
                            response.ResponseObject = ezugiResult.RoundResult[0];
                        break;
                    case GameProviders.Evolution:
                        var evolutionResult = Integration.Products.Helpers.EvolutionHelpers.GetReportByRound(document.ClientId.Value, document.RoundId);
                        if (evolutionResult.RoundResult.Any())
                            response.ResponseObject = evolutionResult.RoundResult[0];
                        break;
					case GameProviders.BGGames:
						var client = CacheManager.GetClientById(document.ClientId.Value);
						var bgGamesResult = Integration.Products.Helpers.BGGamesHelpers.BetHistory(client.PartnerId, document.ClientId.Value, document.ExternalTransactionId);
						response.ResponseObject = bgGamesResult;
						break;
					default:
                        break;
                }
                response.Documents = childs.Select(x => new ApiBetInfoItem
                {
                    Id = x.Id,
                    OperationTypeId = x.OperationTypeId,
                    CreationTime = x.CreationTime.GetGMTDateFromUTC(identity.TimeZone),
                    RoundId = x.RoundId
                }).ToList();

                return response;
            }
        }
        public static ApiResponseBase GetOnlineUsers(int? userId, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetOnlineUsers(userId).Select(x => x.MapToApiOnlineUser(identity.TimeZone)).ToList()
                };
            }
        }

        public static ApiResponseBase GetReportByCredit(SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                using (var clientBl = new ClientBll(identity, log))
                {
                    var resultList = userBl.GetAgentAccountsInfo(identity.Id);
                    resultList.AddRange(clientBl.GetClientsAccountsInfo(identity.Id));
                    var parentAvailableBalance = userBl.GetUserBalance(identity.Id);
                    return new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            Cash = parentAvailableBalance.Balance,
                            YesterdayCash = 0,
                            Total = 0, //?
                            YesterdayTotal = 0, //??
                            PendingTransfer = 0, //?
                            TodayWinLoss = 0,
                            YesterdayWinLoss = 0,
                            GivenCredit = parentAvailableBalance.Credit,
                            TotalAgentCredit = resultList.Sum(x => x.AgentMaxCredit)
                        }
                    };
                }
            }
        }

        private static ApiResponseBase GetBetShopBetsReportPaging(ApiFilterBetShopBet input, SessionIdentity identity, ILog log)
        {
            var user = CacheManager.GetUserById(identity.Id);
            using (var reportBl = new ReportBll(identity, log))
            {
                using (var userBl = new UserBll(identity, log))
                {
                    var isAgentEmploye = user.Type == (int)UserTypes.AgentEmployee;
                    if (isAgentEmploye)
                        userBl.CheckPermission(Constants.Permissions.ViewBetShop);

                    var filter = input.MapToFilterBetShopBet();
                    filter.AgentId = isAgentEmploye ? user.ParentId.Value : identity.Id;
                    var result = reportBl.GetBetshopBetsPagedModel(filter, string.Empty, string.Empty, false);

                    return new ApiResponseBase
                    {
                        ResponseObject = result.MapToBetshopBetsReportModel(reportBl.GetUserIdentity().TimeZone)
                    };
                }
            }
        }

        private static ApiResponseBase GetReportByProviders(ApiFilterReportByProvider filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetReportByProviders(filter.MapToFilterReportByProvider());

                return new ApiResponseBase
                {
                    ResponseObject = result.Select(x => x.MapToApiReportByProvidersElement()).ToList()
                };
            }
        }
    }
}