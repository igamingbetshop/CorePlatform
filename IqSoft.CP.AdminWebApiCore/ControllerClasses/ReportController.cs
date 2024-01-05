using System.Linq;
using Newtonsoft.Json;
using IqSoft.CP.Common;
using IqSoft.CP.DAL;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Filters.Bets;
using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.AdminWebApi.Filters.Reporting;
using IqSoft.CP.AdminWebApi.Models.BetShopModels;
using IqSoft.CP.DAL.Models.Report;
using IqSoft.CP.AdminWebApi.Models.ReportModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop;
using IqSoft.CP.AdminWebApi.Models.ReportModels.Internet;
using log4net;
using IqSoft.CP.Common.Models;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Helpers;
using static IqSoft.CP.Common.Constants;
using System;
using IqSoft.CP.AdminWebApi.Filters.PaymentRequests;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Filters.Reporting;
using Microsoft.AspNetCore.Hosting;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class ReportController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            switch (request.Method)
            {
                case "GetObjectChangeHistory":
                    return GetObjectChangeHistory(JsonConvert.DeserializeObject<ObjectModel>(request.RequestData), identity, log);
                case "GetInternetBetsReportPaging":
                    return
                        GetInternetBetsReportPaging(
                            JsonConvert.DeserializeObject<ApiFilterInternetBet>(request.RequestData), identity, log);
                case "GetInternetBetsByClientReportPaging":
                    return
                        GetInternetBetsByClientReportPaging(
                            JsonConvert.DeserializeObject<ApiFilterInternetBet>(request.RequestData), identity, log);
                case "GetReportByInternetGames":
                    return GetReportByInternetGames(JsonConvert.DeserializeObject<ApiFilterInternetBet>(request.RequestData),
                        identity, log);
                case "ExportReportByInternetGames":
                    return ExportReportByInternetGames(JsonConvert.DeserializeObject<ApiFilterInternetBet>(request.RequestData),
                        identity, log, env);
                case "GetBetShopBetsDashboard":
                    return
                        GetBetShopBetsDashboard(
                            JsonConvert.DeserializeObject<ApiFilterBetShopBet>(request.RequestData), identity, log);
                case "GetBetShopBetsReportPaging":
                    return
                        GetBetShopBetsReportPaging(
                            JsonConvert.DeserializeObject<ApiFilterBetShopBet>(request.RequestData), identity, log);
                case "GetReportByBetShopPayments":
                    return
                        GetReportByBetShopPayments(
                            JsonConvert.DeserializeObject<ApiFilterReportByBetShopPayment>(request.RequestData),
                            identity, log);
                case "GetReportByBetShops":
                    return GetReportByBetShops(JsonConvert.DeserializeObject<ApiFilterReportByBetShop>(request.RequestData),
                        identity, log);
                case "GetReportByBetShopGames":
                    return GetReportByBetShopGames(JsonConvert.DeserializeObject<ApiFilterBetShopBet>(request.RequestData),
                        identity, log);
                case "GetBetShopReconings":
                    return
                        GetBetShopReconings(
                            JsonConvert.DeserializeObject<ApiFilterBetShopReconing>(request.RequestData), identity, log);
                case "GetCashDeskTransactionsPage":
                    return
                        GetCashDeskTransactionsPage(
                            JsonConvert.DeserializeObject<ApiFilterCashDeskTransaction>(request.RequestData), identity, log);

                case "GetReportByProviders":
                    return
                        GetReportByProviders(
                            JsonConvert.DeserializeObject<ApiFilterReportByProvider>(request.RequestData), identity, log);
                case "GetReportByProducts":
                    return
                        GetReportByProducts(
                            JsonConvert.DeserializeObject<ApiFilterReportByProduct>(request.RequestData), identity, log);

                case "GetReportByUserLogsPaging":
                    return
                        GetReportByUserLogsPaging(
                            JsonConvert.DeserializeObject<ApiFilterReportByActionLog>(request.RequestData), identity, log);
                case "GetReportByObjectChangeHistory":
                    return
                        GetReportByObjectChangeHistory(
                            JsonConvert.DeserializeObject<ApiFilterReportByObjectChangeHistory>(request.RequestData), identity, log);
                case "GetPartnerPaymentsSummaryReport":
                    return
                        GetPartnerPaymentsSummaryReport(
                            JsonConvert.DeserializeObject<ApiFilterPartnerPaymentsSummary>(request.RequestData),
                            identity, log);

                case "GetBetInfo":
                    return GetBetInfo(JsonConvert.DeserializeObject<long>(request.RequestData), identity, log);

                case "ExportInternetBetsByClient":
                    return
                        ExportInternetBetsByClient(
                            JsonConvert.DeserializeObject<ApiFilterInternetBet>(request.RequestData), identity, log, env);

                case "ExportBetShopBets":
                    return ExportBetShopBets(JsonConvert.DeserializeObject<ApiFilterBetShopBet>(request.RequestData),
                        identity, log, env);

                case "ExportByBetShopPayments":
                    return
                        ExportByBetShopPayments(
                            JsonConvert.DeserializeObject<ApiFilterReportByBetShopPayment>(request.RequestData),
                            identity, log, env);

                case "ExportInternetBet":
                    return ExportInternetBet(JsonConvert.DeserializeObject<ApiFilterInternetBet>(request.RequestData),
                        identity, log, env);

                case "ExportProducts":
                    return ExportProducts(JsonConvert.DeserializeObject<ApiFilterReportByProduct>(request.RequestData),
                        identity, log, env);

                case "ExportBetShops":
                    return ExportBetShops(JsonConvert.DeserializeObject<ApiFilterBetShopBet>(request.RequestData),
                        identity, log, env);

                case "ExportProviders":
                    return ExportProviders(
                        JsonConvert.DeserializeObject<ApiFilterReportByProvider>(request.RequestData), identity, log, env);

                case "ExportBetShopReconings":
                    return
                        ExportBetShopReconings(
                            JsonConvert.DeserializeObject<ApiFilterBetShopReconing>(request.RequestData), identity, log, env);

                case "ExportByUserLogs":
                    return ExportByUserLogs(
                        JsonConvert.DeserializeObject<ApiFilterReportByActionLog>(request.RequestData), identity, log, env);
                case "ExportClientSessions":
                    return ExportClientSessions(
                        JsonConvert.DeserializeObject<ApiFilterReportByClientSession>(request.RequestData), identity, log, env);
                case "ExportObjectChangeHistory":
                    return ExportObjectChangeHistory(
                        JsonConvert.DeserializeObject<ApiFilterReportByObjectChangeHistory>(request.RequestData), identity, log, env);
                case "GetReportByCorrections":
                    return GetReportByCorrections(
                        JsonConvert.DeserializeObject<ApiFilterClientCorrection>(request.RequestData), identity, log);
                case "GetReportByBetShopLimitChanges":
                    return GetReportByBetShopLimitChanges(
                        JsonConvert.DeserializeObject<ApiFilterBetShopLimitChanges>(request.RequestData), identity, log);
                case "GetReportByClientIdentity":
                    return GetReportByClientIdentity(JsonConvert.DeserializeObject<ApiFilterReportByClientIdentity>(request.RequestData), identity, log);
                case "ExportClientIdentities":
                    return ExportClientIdentities(JsonConvert.DeserializeObject<ApiFilterReportByClientIdentity>(request.RequestData), identity, log, env);
                case "GetReportByPaymentSystems":
                    return GetReportByPaymentSystems(JsonConvert.DeserializeObject<ApiFilterReportByPaymentSystem>(request.RequestData), identity, log);
                case "ExportReportByPaymentSystems":
                    return ExportReportByPaymentSystems(JsonConvert.DeserializeObject<ApiFilterReportByPaymentSystem>(request.RequestData), identity, log, env);
                case "GetReportBySegment":
                    return GetReportBySegment(JsonConvert.DeserializeObject<FilterReportBySegment>(request.RequestData), identity, log);
                case "GetReportByPartners":
                    return GetReportByPartners(JsonConvert.DeserializeObject<ApiFilterReportByPartner>(request.RequestData), identity, log);
                case "ExportReportByPartners":
                    return ExportReportByPartners(JsonConvert.DeserializeObject<ApiFilterReportByPartner>(request.RequestData), identity, log, env);
                case "GetReportByAgentTransfers":
                    return GetReportByAgentTransfers(JsonConvert.DeserializeObject<ApiFilterReportByAgentTranfer>(request.RequestData), identity, log);
                case "ExportReportByAgentTransfers":
                    return ExportReportByAgentTransfers(JsonConvert.DeserializeObject<ApiFilterReportByAgentTranfer>(request.RequestData), identity, log, env);
                case "GetReportByUserTransactions":
                    return GetReportByUserTransactions(JsonConvert.DeserializeObject<ApiFilterReportByUserTransaction>(request.RequestData), identity, log);
                case "ExportReportByUserTransactions":
                    return ExportReportByUserTransactions(JsonConvert.DeserializeObject<ApiFilterReportByUserTransaction>(request.RequestData), identity, log, env);
                case "GetReportByClientSessions":
                    return GetReportByClientSessions(JsonConvert.DeserializeObject<ApiFilterReportByClientSession>(request.RequestData), identity, log);
                case "GetClientSessionInfo":
                    return GetClientSessionInfo(Convert.ToInt64(request.RequestData), identity, log);
                case "GetReportByBonus":
                    return GetReportByBonuses(JsonConvert.DeserializeObject<ApiFilterReportByBonus>(request.RequestData), identity, log);
                case "ExportReportByBonuses":
                    return ExportReportByBonuses(JsonConvert.DeserializeObject<ApiFilterReportByBonus>(request.RequestData), identity, log, env);
                case "GetObjectHistoryElementById":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetObjectHistoryElementById(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "GetReportByLogs":
                    return GetReportByLogs(JsonConvert.DeserializeObject<ApiFilterReportByLog>(request.RequestData), identity, log);
                case "GetReportByClientExclusions":
                    return GetReportByClientExclusions(JsonConvert.DeserializeObject<ApiFilterReportByClientExclusion>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Errors.MethodNotFound);
        }

        private static ApiResponseBase GetObjectHistoryElementById(long objectHistoryId, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase
                {
                    ResponseObject = reportBl.GetObjectHistoryElementById(objectHistoryId)
                };
                return response;
            }
        }

        private static ApiResponseBase GetObjectChangeHistory(ObjectModel input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var response = new ApiResponseBase
                {
                    ResponseObject = reportBl.GetObjectChangeHistory(input.ObjectTypeId, input.ObjectId).Select(x => x.MapToClientHistoryModel(identity.TimeZone)).ToList()
                };
                return response;
            }
        }

        private static ApiResponseBase GetInternetBetsReportPaging(ApiFilterInternetBet input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, null, 120))
            {
                var filter = input.MapToFilterInternetBet();
                var bets = reportBl.GetInternetBetsPagedModel(filter, string.Empty, true);
                var apibets = bets.MapToInternetBetsReportModel(reportBl.GetUserIdentity().TimeZone);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Bets = apibets,
                        TotalWinAmount = bets.TotalWinAmount,
                        TotalBetAmount = bets.TotalBetAmount,
                        TotalProfit = bets.TotalGGR,
                        TotalPlayersCount = bets.TotalPlayersCount,
                        TotalProvidersCount = bets.TotalProvidersCount,
                        TotalPossibleWinAmount = bets.TotalPossibleWinAmount,
                        TotalProductsCount = bets.TotalProductsCount
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase GetInternetBetsByClientReportPaging(ApiFilterInternetBet input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterInternetBet();
                var bets = reportBl.GetInternetBetsByClientPagedModel(filter);
                var apibets = bets.MapToApiInternetBetsByClient();
                var response = new ApiResponseBase
                {
                    ResponseObject = apibets
                };
                return response;
            }
        }

        private static ApiResponseBase GetReportByInternetGames(ApiFilterInternetBet filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, null, 120))
            {
                if ((filter.BetDateBefore - filter.BetDateFrom).TotalDays > 40)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.InvalidDataRange);
                var result = reportBl.GetReportByInternetGames(filter.MapToFilterInternetGame()).ToApiInternetGamesReport();


                return new ApiResponseBase
                {
                    ResponseObject = result
                };
            }
        }

        private static ApiResponseBase ExportReportByInternetGames(ApiFilterInternetBet filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                if ((filter.BetDateBefore - filter.BetDateFrom).TotalDays > 40)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.InvalidDataRange);
                var result = reportBl.ExportReportByInternetGames(filter.MapToFilterInternetGame()).Entities.Select(x => x.ToApiInternetGame()).ToList();
                var fileName = "ExportReportByInternetGames.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.BetDateFrom, filter.BetDateBefore, identity.TimeZone, env);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase GetBetShopBetsDashboard(ApiFilterBetShopBet input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterBetShopBet();
                var result = reportBl.GetBetshopBetsPagedModel(filter, string.Empty, Permissions.ViewBetShopBetsDashboard);

                return new ApiResponseBase
                {
                    ResponseObject = result.MapToBetshopBetsReportModel(reportBl.GetUserIdentity().TimeZone)
                };
            }
        }

        private static ApiResponseBase GetBetShopBetsReportPaging(ApiFilterBetShopBet input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterBetShopBet();
                var result = reportBl.GetBetshopBetsPagedModel(filter, string.Empty, Permissions.ViewBetShopBets);

                return new ApiResponseBase
                {
                    ResponseObject = result.MapToBetshopBetsReportModel(reportBl.GetUserIdentity().TimeZone)
                };
            }
        }

        private static ApiResponseBase GetReportByBetShopPayments(ApiFilterReportByBetShopPayment input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterReportByBetShopPayment();
                var result = reportBl.GetReportByBetShopPayments(filter);

                return new ApiResponseBase
                {
                    ResponseObject = result.Select(x => x.MapToApiReportByBetShopPaymentsElement()).ToList()
                };
            }
        }


        private static ApiResponseBase GetReportByBetShops(ApiFilterReportByBetShop filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                if ((filter.BetDateBefore - filter.BetDateFrom).TotalDays > 40)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.InvalidDataRange);
                var result = reportBl.GetReportByBetShops(filter.MapToFilterBetShopBet()).MapToBetShopsReportModel();


                return new ApiResponseBase
                {
                    ResponseObject = result
                };
            }
        }

        private static ApiResponseBase GetReportByBetShopGames(ApiFilterBetShopBet filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                if ((filter.BetDateBefore - filter.BetDateFrom).TotalDays > 40)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.InvalidDataRange);
                var result = reportBl.GetReportByBetShopGames(filter.MapToFilterBetShopBet()).ToApiBetShopGamesReport();

                return new ApiResponseBase
                {
                    ResponseObject = result
                };
            }
        }

        private static ApiResponseBase GetBetShopReconings(ApiFilterBetShopReconing filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetBetShopReconingsPage(filter.MapToFilterBetShopReconing());
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.MapToBetShopReconingModels(reportBl.GetUserIdentity().TimeZone),
                        result.TotalAmount,
                        result.TotalBalance
                    }
                };
            }
        }

        private static ApiResponseBase GetCashDeskTransactionsPage(ApiFilterCashDeskTransaction filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetCashDeskTransactionsPage(filter.MapToFilterCashDeskTransaction());

                return new ApiResponseBase
                {
                    ResponseObject = result.MapToCashdeskTransactionsReportModel(identity.TimeZone)
                };
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
        private static ApiResponseBase GetReportBySegment(FilterReportBySegment input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetReportBySegment(input)
                };
            }
        }

        private static ApiResponseBase GetReportByPaymentSystems(ApiFilterReportByPaymentSystem filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetReportByPaymentSystems(filter.MapToFilterReportByPaymentSystem(), filter.Type);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.MapToFilterReportByPaymentSystem()).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase ExportReportByPaymentSystems(ApiFilterReportByPaymentSystem filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportReportByPaymentSystems(filter.MapToFilterReportByPaymentSystem(), (int)PaymentRequestTypes.Deposit).Entities.ToList();
                var fileName = "ExportReportByPaymentSystems.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, env);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByPartners(ApiFilterReportByPartner filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetReportByPartners(filter.MapToFilterReportByPartner()).Select(x => x.MapToFilterReportByPartner()).ToList()
                };
            }
        }

        private static ApiResponseBase ExportReportByPartners(ApiFilterReportByPartner filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportReportByPartners(filter.MapToFilterReportByPartner());
                var fileName = "ExportReportByPartners.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, env);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByAgentTransfers(ApiFilterReportByAgentTranfer filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetReportByAgentTransfers(filter.MapToFilterReportByAgentTranfer());

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.MapToFilterReportByAgentTranfer()).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase ExportReportByAgentTransfers(ApiFilterReportByAgentTranfer filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportReportByAgentTransfers(filter.MapToFilterReportByAgentTranfer()).Entities.ToList();
                var fileName = "ExportReportByAgentTranfers.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, env);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByUserTransactions(ApiFilterReportByUserTransaction filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetReportByUserTransactions(filter.MapToFilterReportByUserTransaction());

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.MapToApiReportByUserTransaction()).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase ExportReportByUserTransactions(ApiFilterReportByUserTransaction filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportReportByUserTransactions(filter.MapToFilterReportByUserTransaction()).Entities.ToList();
                var fileName = "ExportReportByUserTransactions.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, env);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByProducts(ApiFilterReportByProduct filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetReportByProducts(filter.MapToFilterReportByProduct());

                return new ApiResponseBase
                {
                    ResponseObject = result.MapToApiReportByProductsElements()
                };
            }
        }

        private static ApiResponseBase GetReportByUserLogsPaging(ApiFilterReportByActionLog filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetReportByActionLogPaging(filter.MapToFilterReportByActionLog(), true);

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

        private static ApiResponseBase GetPartnerPaymentsSummaryReport(ApiFilterPartnerPaymentsSummary filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var request = filter.MapToFilterPartnerPaymentsSummary();
                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        PaymentRequests = reportBl.GetPartnerPaymentsSummaryReport(request)//Map Should be implemented
                    }
                };
                return response;
            }
        }

        public static ApiGetBetInfoResponse GetBetInfo(long betId, SessionIdentity identity, ILog log)
        {
            using (var documentBl = new DocumentBll(identity, log))
            {
                var response = new ApiGetBetInfoResponse();
                var document = documentBl.GetDocumentById(betId, "Client");
                if (document == null || document.ProductId == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.DocumentNotFound);
                var childs = documentBl.GetDocumentsByParentId(document.Id);

                var product = CacheManager.GetProductById(document.ProductId.Value);
                var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);

                switch (provider.Name)
                {
                    case GameProviders.IqSoft:
                    case GameProviders.Internal:
                        HttpRequestInput requestObject;
                        if (provider.Name.ToLower() == GameProviders.IqSoft.ToLower())
                        {
                            var pKey = CacheManager.GetPartnerSettingByKey(document.Client.PartnerId, PartnerKeys.IqSoftBrandId);
                            requestObject = Integration.Products.Helpers.IqSoftHelpers.GetBetInfo(pKey.StringValue, provider, document.ExternalTransactionId, identity.LanguageId, product.ExternalId);
                        }
                        else
                            requestObject = Integration.Products.Helpers.InternalHelpers.GetBetInfo(product, document.ExternalTransactionId, identity.LanguageId, product.ExternalId);
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
                    case GameProviders.BlueOcean:
                        response.ResponseObject = new
                        {
                            Url = Integration.Products.Helpers.BlueOceanHelpers.GetGameReport(document.ClientId.Value, document.ProductId, document.RoundId),
                            document.RoundId,
                            document.CreationTime
                        };
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

        private static ApiResponseBase ExportInternetBet(ApiFilterInternetBet input, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log, null, 120))
            {
                var timeZone = reportBl.GetUserIdentity().TimeZone;
                var filter = input.MapToFilterInternetBet();
                var filteredList = reportBl.ExportInternetBet(filter).Select(x => x.MapToInternetBetModel(timeZone)).ToList();
                string fileName = "ExportInternetBet.csv";
                string fileAbsPath = reportBl.ExportToCSV<InternetBetModel>(fileName, filteredList, input.BetDateFrom, input.BetDateBefore, timeZone, env);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase ExportInternetBetsByClient(ApiFilterInternetBet input, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterInternetBet();
                var bets = reportBl.ExportInternetBetsByClient(filter);
                var filteredList = bets.Select(x => x.MapToApiInternetBetByClient()).ToList();
                string fileName = "ExportInternetBetsByClient.csv";
                string fileAbsPath = reportBl.ExportToCSV<ApiInternetBetByClient>(fileName, filteredList, input.BetDateFrom, 
                                                                                  input.BetDateBefore, reportBl.GetUserIdentity().TimeZone, env);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase ExportBetShopBets(ApiFilterBetShopBet input, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var timeZone = reportBl.GetUserIdentity().TimeZone;
                var filter = input.MapToFilterBetShopBet();
                var result = reportBl.ExportBetShopBets(filter);
                var filteredList = result.MapBetShopBets(timeZone);
                string fileName = "ExportBetShopBets.csv";
                string fileAbsPath = reportBl.ExportToCSV<BetShopBetModel>(fileName, filteredList, input.BetDateFrom, input.BetDateBefore, timeZone, env);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase ExportByBetShopPayments(ApiFilterReportByBetShopPayment input, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterReportByBetShopPayment();
                var result = reportBl.ExportByBetShopPayments(filter);
                var filteredList = result.Select(x => x.MapToApiReportByBetShopPaymentsElement()).ToList();
                string fileName = "ExportByBetShopPayments.csv";
                string fileAbsPath = reportBl.ExportToCSV<ApiReportByBetShopPaymentsElement>(fileName, filteredList, input.FromDate, 
                                                                                             input.ToDate, reportBl.GetUserIdentity().TimeZone, env);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase ExportProducts(ApiFilterReportByProduct filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportProducts(filter.MapToFilterReportByProduct());
                string fileName = "ExportProducts.csv";
                string fileAbsPath = reportBl.ExportToCSV<fnReportByProduct>(fileName, result, filter.FromDate, filter.ToDate,
                                                                             reportBl.GetUserIdentity().TimeZone, env);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase ExportBetShops(ApiFilterBetShopBet filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportBetShops(filter.MapToFilterBetShopBet()).Select(x => x.MapToBetShopReportModel()).ToList();
                string fileName = "ExportBetShops.csv";
                string fileAbsPath = reportBl.ExportToCSV<ApiBetShopReport>(fileName, result, filter.BetDateFrom, filter.BetDateBefore, 
                                                                            reportBl.GetUserIdentity().TimeZone, env);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase ExportProviders(ApiFilterReportByProvider filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportProviders(filter.MapToFilterReportByProvider());
                string fileName = "ExportProviders.csv";
                string fileAbsPath = reportBl.ExportToCSV<ReportByProvidersElement>(fileName, result, filter.FromDate, filter.ToDate, 
                                                                                    reportBl.GetUserIdentity().TimeZone, env);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase ExportBetShopReconings(ApiFilterBetShopReconing filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportBetShopReconings(filter.MapToFilterBetShopReconing()).MapToBetShopReconingModels(reportBl.GetUserIdentity().TimeZone);
                string fileName = "ExportBetShopReconings.csv";
                string fileAbsPath = reportBl.ExportToCSV<BetShopReconingModel>(fileName, result, null, null, 0,env);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase ExportByUserLogs(ApiFilterReportByActionLog filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportByActionLogs(filter.MapToFilterReportByActionLog()).MapToApiReportByActionLog(identity.TimeZone);
                string fileName = "ExportByUserLogs.csv";
                string fileAbsPath = reportBl.ExportToCSV<ApiReportByActionLog>(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, env);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase GetReportByCorrections(ApiFilterClientCorrection filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var corrections = reportBl.GetReportByCorrections(filter.MapToFilterCorrection());

                return new ApiResponseBase
                {
                    ResponseObject = corrections.MapToApiClientCorrections(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase GetReportByBetShopLimitChanges(ApiFilterBetShopLimitChanges filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var betShopLimitChanges = reportBl.GetBetShopLimitChangesReport(filter.MapToFilterReportByBetShopLimitChanges());
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        betShopLimitChanges.Count,
                        Entities = betShopLimitChanges.Entities.Select(x => x.MapToApiBetShopLimitChanges(reportBl.GetUserIdentity().TimeZone)).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByClientSessions(ApiFilterReportByClientSession filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetClientSessions(filter.MapToFilterReportByClientSession(), true);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.MapToClientSessionModels(identity.TimeZone)
                    }
                };
            }
        }

        private static ApiResponseBase GetClientSessionInfo(long sessionId, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = clientBl.GetClientSessionInfo(sessionId).MapToClientSessionModels(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase ExportClientSessions(ApiFilterReportByClientSession filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportClientSessions(filter.MapToFilterReportByClientSession()).MapToClientSessionModels(reportBl.GetUserIdentity().TimeZone);
                var fileName = "ExportClientSessions.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, env);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByClientIdentity(ApiFilterReportByClientIdentity filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetReportByClientIdentity(filter.MapToFilterClientIdentity());
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.ToClientIdentityModel(identity.TimeZone)).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase ExportClientIdentities(ApiFilterReportByClientIdentity filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportClientIdentities(filter.MapToFilterClientIdentity()).Entities.ToList();
                var fileName = "ExportClientIdentities.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, env);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByObjectChangeHistory(ApiFilterReportByObjectChangeHistory filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetReportByObjectChangeHistory(filter.MapToFilterObjectChangeHistory());
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.MapToApiReportByObjectChangeHistory(identity.TimeZone)).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase ExportObjectChangeHistory(ApiFilterReportByObjectChangeHistory filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportObjectChangeHistory(filter.MapToFilterObjectChangeHistory())
                    .Entities.Select(x => x.MapToApiReportByObjectChangeHistory(identity.TimeZone)).ToList();
                var fileName = "ExportObjectChangeHistory.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, env);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase GetReportByBonuses(ApiFilterReportByBonus filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetReportByBonus(filter.MapToFilterReportByBonus());
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        result.TotalBonusPrize,
                        result.TotalFinalAmount,
                        Entities = result.Entities.Select(x => x.MapToApiClientBonus(identity.TimeZone)).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase ExportReportByBonuses(ApiFilterReportByBonus filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportReportByBonus(filter.MapToFilterReportByBonus()).Select(x => x.MapToApiClientBonus(identity.TimeZone)).ToList();
                var fileName = "ExportReportByBonus.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, env);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByClientExclusions(ApiFilterReportByClientExclusion filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetReportByClientExclusions(filter.MapToFilterClientExclusion());
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.MapToApiClientExclusion()).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByLogs(ApiFilterReportByLog input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetLogs(input.Id, input.FromDate, input.ToDate, input.TakeCount, input.SkipCount);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => new { x.Id, x.Type, x.Caller, x.Message, x.CreationTime })
                    }
                };
            }
        }
    }
}