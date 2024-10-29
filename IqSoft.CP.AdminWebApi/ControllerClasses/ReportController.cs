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
using IqSoft.CP.Integration.Platforms.Helpers;
using System.Collections.Generic;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.AdminWebApi.Filters.Clients;
using IqSoft.CP.AdminWebApi.Filters.Affiliate;
using IqSoft.CP.Common.Models.Filters;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class ReportController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
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
                        identity, log);
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
                            JsonConvert.DeserializeObject<ApiFilterInternetBet>(request.RequestData), identity, log);

                case "ExportBetShopBets":
                    return ExportBetShopBets(JsonConvert.DeserializeObject<ApiFilterBetShopBet>(request.RequestData),
                        identity, log);

                case "ExportByBetShopPayments":
                    return
                        ExportByBetShopPayments(
                            JsonConvert.DeserializeObject<ApiFilterReportByBetShopPayment>(request.RequestData),
                            identity, log);

                case "ExportInternetBet":
                    return ExportInternetBet(JsonConvert.DeserializeObject<ApiFilterInternetBet>(request.RequestData),
                        identity, log);

                case "ExportProducts":
                    return ExportProducts(JsonConvert.DeserializeObject<ApiFilterReportByProduct>(request.RequestData),
                        identity, log);

                case "ExportBetShops":
                    return ExportBetShops(JsonConvert.DeserializeObject<ApiFilterBetShopBet>(request.RequestData),
                        identity, log);

                case "ExportProviders":
                    return ExportProviders(
                        JsonConvert.DeserializeObject<ApiFilterReportByProvider>(request.RequestData), identity, log);

                case "ExportBetShopReconings":
                    return
                        ExportBetShopReconings(
                            JsonConvert.DeserializeObject<ApiFilterBetShopReconing>(request.RequestData), identity, log);

                case "ExportByUserLogs":
                    return ExportByUserLogs(
                        JsonConvert.DeserializeObject<ApiFilterReportByActionLog>(request.RequestData), identity, log);
                case "ExportClientSessions":
                    return ExportClientSessions(
                        JsonConvert.DeserializeObject<ApiFilterReportByClientSession>(request.RequestData), identity, log);
                case "ExportObjectChangeHistory":
                    return ExportObjectChangeHistory(
                        JsonConvert.DeserializeObject<ApiFilterReportByObjectChangeHistory>(request.RequestData), identity, log);
                case "GetReportByCorrections":
                    return GetReportByCorrections(
                        JsonConvert.DeserializeObject<ApiFilterClientCorrection>(request.RequestData), identity, log);
                case "GetReportByBetShopLimitChanges":
                    return GetReportByBetShopLimitChanges(
                        JsonConvert.DeserializeObject<ApiFilterBetShopLimitChanges>(request.RequestData), identity, log);
                case "GetReportByClientIdentity":
                    return GetReportByClientIdentity(JsonConvert.DeserializeObject<ApiFilterReportByClientIdentity>(request.RequestData), identity, log);
                case "ExportClientIdentities":
                    return ExportClientIdentities(JsonConvert.DeserializeObject<ApiFilterReportByClientIdentity>(request.RequestData), identity, log);
                case "GetReportByPaymentSystems":
                    return GetReportByPaymentSystems(JsonConvert.DeserializeObject<ApiFilterReportByPaymentSystem>(request.RequestData), identity, log);
                case "ExportReportByPaymentSystems":
                    return ExportReportByPaymentSystems(JsonConvert.DeserializeObject<ApiFilterReportByPaymentSystem>(request.RequestData), identity, log);
                case "GetReportBySegment":
                    return GetReportBySegment(JsonConvert.DeserializeObject<FilterReportBySegment>(request.RequestData), identity, log);
                case "GetReportByPartners":
                    return GetReportByPartners(JsonConvert.DeserializeObject<ApiFilterReportByPartner>(request.RequestData), identity, log);
                case "ExportReportByPartners":
                    return ExportReportByPartners(JsonConvert.DeserializeObject<ApiFilterReportByPartner>(request.RequestData), identity, log);
                case "GetReportByUserTransactions":
                    return GetReportByUserTransactions(JsonConvert.DeserializeObject<ApiFilterReportByUserTransaction>(request.RequestData), identity, log);
                case "ExportReportByUserTransactions":
                    return ExportReportByUserTransactions(JsonConvert.DeserializeObject<ApiFilterReportByUserTransaction>(request.RequestData), identity, log);
                case "GetReportByUserCorrections":
                    return GetCorrectionsReportByUser(JsonConvert.DeserializeObject<Filters.Reporting.ApiFilterUserCorrection>(request.RequestData),
                                                      (int)UserTypes.AdminUser, identity, log);
                case "ExportReportByUserCorrections":
                    return ExportCorrectionsReportByUser(JsonConvert.DeserializeObject<Filters.Reporting.ApiFilterUserCorrection>(request.RequestData),
                                                      (int)UserTypes.AdminUser, identity, log);
                case "GetReportByAgentCorrections":
                    return GetCorrectionsReportByUser(JsonConvert.DeserializeObject<Filters.Reporting.ApiFilterUserCorrection>(request.RequestData),
                                                      (int)UserTypes.CompanyAgent, identity, log);
                case "ExportReportByAgentCorrections":
                    return ExportCorrectionsReportByUser(JsonConvert.DeserializeObject<Filters.Reporting.ApiFilterUserCorrection>(request.RequestData),
                                                        (int)UserTypes.CompanyAgent, identity, log);
                case "GetReportByClientSessions":
                    return GetReportByClientSessions(JsonConvert.DeserializeObject<ApiFilterReportByClientSession>(request.RequestData), identity, log);
                case "GetReportByUserSessions":
                    return GetReportByUserSessions(JsonConvert.DeserializeObject<ApiFilterReportByUserSession>(request.RequestData), identity, log);
                case "GetClientSessionInfo":
                    return GetClientSessionInfo(Convert.ToInt64(request.RequestData), identity, log);
                case "GetReportByBonus":
                    return GetReportByBonuses(JsonConvert.DeserializeObject<ApiFilterReportByBonus>(request.RequestData), identity, log);
                case "ExportReportByBonuses":
                    return ExportReportByBonuses(JsonConvert.DeserializeObject<ApiFilterReportByBonus>(request.RequestData), identity, log);
                case "GetObjectHistoryElementById":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetObjectHistoryElementById(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "GetReportByLogs":
                    return GetReportByLogs(JsonConvert.DeserializeObject<ApiFilterReportByLog>(request.RequestData), identity, log);
                case "GetReportByJobLogs":
                    return GetReportByJobLogs(JsonConvert.DeserializeObject<ApiFilterReportByLog>(request.RequestData), identity, log);
                case "ResendBet":
                    return ResendBet(JsonConvert.DeserializeObject<long>(request.RequestData), identity, log);
                case "SettleBet":
                    return SettleBet(JsonConvert.DeserializeObject<InternetBetModel>(request.RequestData), identity, log);
                case "GetReportByClientExclusions":
                    return GetReportByClientExclusions(JsonConvert.DeserializeObject<ApiFilterReportByClientExclusion>(request.RequestData), identity, log);
                case "GetReportByClientsGamesPaging":
                    return
                        GetReportByClientsGamesPaging(JsonConvert.DeserializeObject<ApiFilterClientGame>(request.RequestData), identity, log);
                case "GetDuplicateClients":
                    return GetDuplicateClients(JsonConvert.DeserializeObject<ApiFilterfnDuplicateClient>(request.RequestData), identity, log);
                case "GetAffiliateCorrections":
                    return GetAffiliateCorrections(JsonConvert.DeserializeObject<ApiFilterAffiliateCorrection>(request.RequestData), identity, log);
                case "GetReportByPopupStatistics":
                    return GetReportByPopupStatistics(JsonConvert.DeserializeObject<ApiFilterReportByPopupStatistics>(request.RequestData), identity, log);
                case "ExportReportByPopupStatistics":
                    return ExportReportByPopupStatistics(JsonConvert.DeserializeObject<ApiFilterReportByPopupStatistics>(request.RequestData), identity, log);
                case "GetEmails":
                    if (!string.IsNullOrEmpty(request.RequestData))
                        return GetEmails(JsonConvert.DeserializeObject<ApiFilterObjectMessage>(request.RequestData), identity, log);
                    return GetEmails(new ApiFilterObjectMessage(), identity, log);
                case "GetSmses":
                    if (!string.IsNullOrEmpty(request.RequestData))
                        return GetSmses(JsonConvert.DeserializeObject<ApiFilterObjectMessage>(request.RequestData), identity, log);
                    return GetSmses(new ApiFilterObjectMessage(), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Errors.MethodNotFound);
        }

        private static ApiResponseBase GetObjectHistoryElementById(long objectHistoryId, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetObjectHistoryElementById(objectHistoryId)
                };
            }
        }

        private static ApiResponseBase GetObjectChangeHistory(ObjectModel input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetObjectChangeHistory(input.ObjectTypeId, input.ObjectId).Select(x => x.MapToClientHistoryModel(identity.TimeZone)).ToList()
                };
            }
        }

        private static ApiResponseBase GetEmails(ApiFilterObjectMessage apiFilterClientMessage, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = apiFilterClientMessage.MapToFilterClientMessage(identity.TimeZone);
                filter.Types = new List<int> { (int)ClientMessageTypes.Email, (int)ClientMessageTypes.SecuredEmail };
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetObjectMessages(apiFilterClientMessage.ObjectTypeId, filter)
                };
            }
        }

        private static ApiResponseBase GetSmses(ApiFilterObjectMessage apiFilterClientMessage, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = apiFilterClientMessage.MapToFilterClientMessage(identity.TimeZone);
                filter.Types = new List<int> { (int)ClientMessageTypes.Sms, (int)ClientMessageTypes.SecuredSms };
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetObjectMessages(apiFilterClientMessage.ObjectTypeId, filter)
                };
            }
        }

        private static ApiResponseBase GetInternetBetsReportPaging(ApiFilterInternetBet input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                var filter = input.MapToFilterInternetBet(identity.TimeZone);
                var bets = reportBl.GetInternetBetsPagedModel(filter, string.Empty, true);
                var apibets = bets.MapToInternetBetsReportModel(reportBl.GetUserIdentity().TimeZone);

                return new ApiResponseBase
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
                        TotalProductsCount = bets.TotalProductsCount,
                        TotalBonusBetAmount = bets.TotalBonusBetAmount,
                        TotalBonusWinAmount = bets.TotalBonusWinAmount
                    }
                };
            }
        }

        private static ApiResponseBase GetInternetBetsByClientReportPaging(ApiFilterInternetBet input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterInternetBet(identity.TimeZone);
                var bets = reportBl.GetInternetBetsByClientPagedModel(filter);
                var apibets = bets.MapToApiInternetBetsByClient();
                return new ApiResponseBase
                {
                    ResponseObject = apibets
                };
            }
        }

        private static ApiResponseBase GetReportByInternetGames(ApiFilterInternetBet filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                if ((filter.BetDateBefore - filter.BetDateFrom).TotalDays > 40)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.InvalidDataRange);
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetReportByInternetGames(filter.MapToFilterInternetGame(identity.TimeZone)).ToApiInternetGamesReport()
                };
            }
        }

        private static ApiResponseBase ExportReportByInternetGames(ApiFilterInternetBet filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                if ((filter.BetDateBefore - filter.BetDateFrom).TotalDays > 40)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.InvalidDataRange);
                var result = reportBl.ExportReportByInternetGames(filter.MapToFilterInternetGame(identity.TimeZone)).Entities.Select(x => x.ToApiInternetGame()).ToList();
                var fileName = "ExportReportByInternetGames.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.BetDateFrom, filter.BetDateBefore, identity.TimeZone, filter.AdminMenuId);
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
                var filter = input.MapToFilterBetShopBet(identity.TimeZone);
                var result = reportBl.GetBetshopBetsPagedModel(filter, string.Empty, Permissions.ViewBetShopBetsDashboard, true);

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
                var filter = input.MapToFilterBetShopBet(identity.TimeZone);
                var result = reportBl.GetBetshopBetsPagedModel(filter, string.Empty, Permissions.ViewBetShopBets, true);

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
                var filter = input.MapToFilterReportByBetShopPayment(identity.TimeZone);
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
                var result = reportBl.GetReportByBetShops(filter.MapToFilterBetShopBet(identity.TimeZone)).MapToBetShopsReportModel();


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
                var result = reportBl.GetReportByBetShopGames(filter.MapToFilterBetShopBet(identity.TimeZone)).ToApiBetShopGamesReport();

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
                var result = reportBl.GetCashDeskTransactionsPage(filter.MapToFilterCashDeskTransaction(identity.TimeZone));

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
                var result = reportBl.GetReportByProviders(filter.MapToFilterReportByProvider(identity.TimeZone));

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
                var result = reportBl.GetReportByPaymentSystems(filter.MapToFilterReportByPaymentSystem(identity.TimeZone), filter.Type);

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

        private static ApiResponseBase ExportReportByPaymentSystems(ApiFilterReportByPaymentSystem filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportReportByPaymentSystems(filter.MapToFilterReportByPaymentSystem(identity.TimeZone), (int)PaymentRequestTypes.Deposit).Entities.ToList();
                var fileName = "ExportReportByPaymentSystems.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, filter.AdminMenuId);

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
                    ResponseObject = reportBl.GetReportByPartners(filter.MapToFilterReportByPartner(identity.TimeZone)).Select(x => x.MapToFilterReportByPartner()).ToList()
                };
            }
        }

        private static ApiResponseBase ExportReportByPartners(ApiFilterReportByPartner filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportReportByPartners(filter.MapToFilterReportByPartner(identity.TimeZone));
                var fileName = "ExportReportByPartners.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, filter.AdminMenuId);

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
                var result = reportBl.GetReportByUserTransactions(filter.MapToFilterReportByUserTransaction(identity.TimeZone));

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.MapToApiReportByUserTransaction(filter.TimeZone)).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase ExportReportByUserTransactions(ApiFilterReportByUserTransaction filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportReportByUserTransactions(filter.MapToFilterReportByUserTransaction(identity.TimeZone)).Entities.ToList();
                var fileName = "ExportReportByUserTransactions.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, filter.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase GetCorrectionsReportByUser(Filters.Reporting.ApiFilterUserCorrection filter, int userType, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetCorrectionsReportByUser(userType, filter.MapToFilterReportByUserCorrection(identity.TimeZone), true, 0);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.MapToApiCorrectionsReportByUser()).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase ExportCorrectionsReportByUser( Filters.Reporting.ApiFilterUserCorrection filter, int userType, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportCorrectionsReportByUser(userType, filter.MapToFilterReportByUserCorrection(identity.TimeZone), true, 0);
                var fileName = "ExportCorrectionsReportByUser.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result.Select(x=>x.MapToApiCorrectionsReportByUser()).ToList(),
                                                       filter.FromDate, filter.ToDate, identity.TimeZone, filter.AdminMenuId);

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
                var result = reportBl.GetReportByProducts(filter.MapToFilterReportByProduct(identity.TimeZone));

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
                var result = reportBl.GetReportByActionLogPaging(filter.MapToFilterReportByActionLog(identity.TimeZone), true);

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
                var request = filter.MapToFilterPartnerPaymentsSummary(identity.TimeZone);
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
                var document = documentBl.GetDocumentById(betId);
                if (document == null || document.ProductId == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.DocumentNotFound);
                var childs = documentBl.GetDocumentsByParentId(document.Id);

                var product = CacheManager.GetProductById(document.ProductId.Value);
                var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
                int partnerId = 0;
                if (document.ClientId != null)
                {
                    var client = CacheManager.GetClientById(document.ClientId.Value);
                    partnerId = client.PartnerId;
                }
                else
                {
                    var cashDesk = CacheManager.GetCashDeskById(document.CashDeskId.Value);
                    var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
                    partnerId = betShop.PartnerId;
                }
              
                switch (provider.Name)
                {
                    case GameProviders.IqSoft:
                    case GameProviders.Internal:
                        HttpRequestInput requestObject = null;
                        if (provider.Name.ToLower() == GameProviders.IqSoft.ToLower())
                        {
                            var pKey = CacheManager.GetPartnerSettingByKey(partnerId, PartnerKeys.IqSoftBrandId);
                            requestObject = Integration.Products.Helpers.IqSoftHelpers.GetBetInfo(pKey.StringValue, provider, document.ExternalTransactionId,
                                identity.LanguageId, product.ExternalId, identity.PartnerId);
                        }
                        else
                        {
                            requestObject = Integration.Products.Helpers.InternalHelpers.GetBetInfo(product, document.ExternalTransactionId,
                                identity.LanguageId, product.ExternalId, partnerId);
                        }
                        if (requestObject != null)
                            response = JsonConvert.DeserializeObject<ApiGetBetInfoResponse>(CommonFunctions.SendHttpRequest(requestObject, out _));

                        response.Documents = childs.Select(x => new ApiBetInfoItem
                        {
                            Id = x.Id,
                            OperationTypeId = x.OperationTypeId,
                            CreationTime = x.CreationTime.GetGMTDateFromUTC(identity.TimeZone),
                            RoundId = x.RoundId,
                            ExternalId = x.ExternalTransactionId
                        }).ToList();
                        break;
                    case GameProviders.Ezugi:
                        var ezugiResult = Integration.Products.Helpers.EzugiHelpers.GetReport(document.RoundId.Split('-')[0], document.CreationTime);
                        if (ezugiResult.RoundResult.Any())
                            response.ResponseObject = ezugiResult.RoundResult[0];
                        break;
                    case GameProviders.Evolution:
                        try
                        {
                            var evolutionResult = Integration.Products.Helpers.EvolutionHelpers.GetReportByRound(document.ClientId.Value, document.RoundId);
                            if (evolutionResult.RoundResult.Any())
                                response.ResponseObject = evolutionResult.RoundResult[0];
                        }
                        catch(Exception e)
                        {
                            log.Error(e);
                        }
                        break;
                    case GameProviders.BlueOcean:
                        response.ResponseObject = new
                        {
                            Url = Integration.Products.Helpers.BlueOceanHelpers.GetGameReport(document.ClientId.Value, document.ProductId, document.RoundId),
                            document.RoundId,
                            document.CreationTime,
                            ExternalId = document.ExternalTransactionId
                        };
                        break;
                    case GameProviders.BGGames:
						var bgGamesResult = Integration.Products.Helpers.BGGamesHelpers.BetHistory(partnerId, document.ClientId.Value, document.ExternalTransactionId);
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
                    RoundId = x.RoundId,
                    ExternalId = x.ExternalTransactionId
                }).ToList();

                return response;
            }
        }

        private static ApiResponseBase ExportInternetBet(ApiFilterInternetBet input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                var timeZone = reportBl.GetUserIdentity().TimeZone;
                var filter = input.MapToFilterInternetBet(identity.TimeZone);
                var filteredList = reportBl.ExportInternetBet(filter).Select(x => x.MapToInternetBetModel(timeZone)).ToList();
                string fileName = "ExportInternetBet.csv";
                string fileAbsPath = reportBl.ExportToCSV<InternetBetModel>(fileName, filteredList, input.BetDateFrom, input.BetDateBefore, timeZone, input.AdminMenuId);

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

        private static ApiResponseBase ExportInternetBetsByClient(ApiFilterInternetBet input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterInternetBet(identity.TimeZone);
                var bets = reportBl.ExportInternetBetsByClient(filter);
                var filteredList = bets.Select(x => x.MapToApiInternetBetByClient()).ToList();
                string fileName = "ExportInternetBetsByClient.csv";
                string fileAbsPath = reportBl.ExportToCSV<ApiInternetBetByClient>(fileName, filteredList, input.BetDateFrom, input.BetDateBefore, 
                                                                                  identity.TimeZone, input.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase ExportBetShopBets(ApiFilterBetShopBet input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var timeZone = reportBl.GetUserIdentity().TimeZone;
                var filter = input.MapToFilterBetShopBet(identity.TimeZone);
                var result = reportBl.ExportBetShopBets(filter);
                var filteredList = result.Select(x => x.MapToBetShopBet(timeZone)).ToList();
                string fileName = "ExportBetShopBets.csv";
                string fileAbsPath = reportBl.ExportToCSV<BetShopBetModel>(fileName, filteredList, input.BetDateFrom, input.BetDateBefore, timeZone, input.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase ExportByBetShopPayments(ApiFilterReportByBetShopPayment input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var filter = input.MapToFilterReportByBetShopPayment(identity.TimeZone);
                var result = reportBl.ExportByBetShopPayments(filter);
                var filteredList = result.Select(x => x.MapToApiReportByBetShopPaymentsElement()).ToList();
                string fileName = "ExportByBetShopPayments.csv";
                string fileAbsPath = reportBl.ExportToCSV<ApiReportByBetShopPaymentsElement>(fileName, filteredList, input.FromDate, input.ToDate, 
                                                                                             identity.TimeZone, input.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase ExportProducts(ApiFilterReportByProduct filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportProducts(filter.MapToFilterReportByProduct(identity.TimeZone));
                string fileName = "ExportProducts.csv";
                string fileAbsPath = reportBl.ExportToCSV<fnReportByProduct>(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, filter.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase ExportBetShops(ApiFilterBetShopBet filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportBetShops(filter.MapToFilterBetShopBet(identity.TimeZone)).Select(x => x.MapToBetShopReportModel()).ToList();
                string fileName = "ExportBetShops.csv";
                string fileAbsPath = reportBl.ExportToCSV<ApiBetShopReport>(fileName, result, filter.BetDateFrom, filter.BetDateBefore,
                                                                            identity.TimeZone, filter.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase ExportProviders(ApiFilterReportByProvider filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportProviders(filter.MapToFilterReportByProvider(identity.TimeZone));
                string fileName = "ExportProviders.csv";
                string fileAbsPath = reportBl.ExportToCSV<ReportByProvidersElement>(fileName, result, filter.FromDate, filter.ToDate, 
                                                                                    identity.TimeZone, filter.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase ExportBetShopReconings(ApiFilterBetShopReconing filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportBetShopReconings(filter.MapToFilterBetShopReconing()).MapToBetShopReconingModels(identity.TimeZone);
                string fileName = "ExportBetShopReconings.csv";
                string fileAbsPath = reportBl.ExportToCSV<BetShopReconingModel>(fileName, result, null, null, 0, filter.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase ExportByUserLogs(ApiFilterReportByActionLog filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportByActionLogs(filter.MapToFilterReportByActionLog(identity.TimeZone)).MapToApiReportByActionLog(identity.TimeZone);
                string fileName = "ExportByUserLogs.csv";
                string fileAbsPath = reportBl.ExportToCSV<ApiReportByActionLog>(fileName, result, filter.FromDate, filter.ToDate, 
                                                                                identity.TimeZone, filter.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByCorrections(ApiFilterClientCorrection filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var corrections = reportBl.GetReportByCorrections(filter.MapToFilterCorrection(identity.TimeZone));

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
                var betShopLimitChanges = reportBl.GetBetShopLimitChangesReport(filter.MapToFilterReportByBetShopLimitChanges(identity.TimeZone));
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
                var result = reportBl.GetClientSessions(filter.MapToFilterReportByfnClientSession(identity.TimeZone), true);

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

        private static ApiResponseBase GetReportByUserSessions(ApiFilterReportByUserSession filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetUserSessions(filter.MapToFilterReportByUserSession(identity.TimeZone));

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        result.Count,
                        Entities = result.Entities.Select(x => x.MapToUserSessionModel(identity.TimeZone)).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase GetClientSessionInfo(long sessionId, SessionIdentity identity, ILog log)
        {
            using (var reportBll = new ReportBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = reportBll.GetClientSessionInfo(sessionId).Select(x=> x.MapToClientSessionModel(identity.TimeZone)).ToList()
                };
            }
        }

        private static ApiResponseBase ExportClientSessions(ApiFilterReportByClientSession filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportClientSessions(filter.MapToFilterReportByfnClientSession(identity.TimeZone)).MapToClientSessionModels(identity.TimeZone);
                var fileName = "ExportClientSessions.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, filter.AdminMenuId);

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
                var result = reportBl.GetReportByClientIdentity(filter.MapToFilterClientIdentity(identity.TimeZone));
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

        private static ApiResponseBase ExportClientIdentities(ApiFilterReportByClientIdentity filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportClientIdentities(filter.MapToFilterClientIdentity(identity.TimeZone)).Entities.ToList();
                var fileName = "ExportClientIdentities.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, filter.AdminMenuId);

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
                var result = reportBl.GetReportByObjectChangeHistory(filter.MapToFilterObjectChangeHistory(identity.TimeZone));
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

        private static ApiResponseBase ExportObjectChangeHistory(ApiFilterReportByObjectChangeHistory filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportObjectChangeHistory(filter.MapToFilterObjectChangeHistory(identity.TimeZone))
                    .Entities.Select(x => x.MapToApiReportByObjectChangeHistory(identity.TimeZone)).ToList();
                var fileName = "ExportObjectChangeHistory.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, filter.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByBonuses(ApiFilterReportByBonus filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.GetReportByBonus(filter.MapToFilterReportByBonus(identity.TimeZone));
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

        private static ApiResponseBase ExportReportByBonuses(ApiFilterReportByBonus filter, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                var result = reportBl.ExportReportByBonus(filter.MapToFilterReportByBonus(identity.TimeZone)).Select(x => x.MapToApiClientBonus(identity.TimeZone)).ToList();
                var fileName = "ExportReportByBonus.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, filter.FromDate, filter.ToDate, identity.TimeZone, filter.AdminMenuId);
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

        private static ApiResponseBase GetReportByJobLogs(ApiFilterReportByLog input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = reportBl.GetJobLogs(input.Id, input.JobId, input.FromDate, input.ToDate, input.TakeCount, input.SkipCount)
                };
            }
        }

        private static ApiResponseBase ResendBet(long betId, SessionIdentity identity, ILog log)
        {
            try
            {
                using (var documentBl = new DocumentBll(identity, log))
                {
                    var documents = documentBl.GetBetRelatedDocuments(betId);
                    var betStatus = documentBl.GetBetStatus(betId);
                    var betDocument = documents.First(x => x.OperationTypeId == (int)OperationTypes.Bet);
                    var client = CacheManager.GetClientById(betDocument.ClientId.Value);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformInfo);
                    if (!isExternalPlatformClient)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DocumentNotFound);
                    if (betStatus == (int)BetDocumentStates.Won || betStatus == (int)BetDocumentStates.Lost ||
                        betStatus == (int)BetDocumentStates.Cashouted || betStatus == (int)BetDocumentStates.Returned || betStatus == (int)BetDocumentStates.Deleted)
                    {
                        var winDocument = documents.FirstOrDefault(x => x.OperationTypeId == (int)OperationTypes.Win || x.OperationTypeId == (int)OperationTypes.CashOut);
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = null,
                            CurrencyId = client.CurrencyId,
                            ProductId = betDocument.ProductId,
                            CreditTransactionId = betDocument.Id,
                            State = betStatus,
                            OperationItems = new List<OperationItemFromProduct>
                                {
                                    new OperationItemFromProduct
                                    {
                                        Client = client
                                    }
                                }
                        };
                        if (winDocument != null)
                        {
                            operationsFromProduct.RoundId = winDocument.RoundId;
                            operationsFromProduct.GameProviderId = winDocument.GameProviderId.Value;
                            operationsFromProduct.OperationTypeId = winDocument.OperationTypeId;
                            operationsFromProduct.TransactionId = winDocument.Id.ToString();
                            operationsFromProduct.Info = winDocument.Info;
                            operationsFromProduct.TicketInfo = winDocument.Info;
                            operationsFromProduct.OperationItems = new List<OperationItemFromProduct>
                            {
                                new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = winDocument.Amount
                                }
                            };

                            if (betStatus == (int)BetDocumentStates.Deleted)
                            {
                                var rollbackDocument = documents.First(x => x.OperationTypeId == (int)OperationTypes.Rollback || x.OperationTypeId == (int)OperationTypes.WinRollback);
                                ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformInfo.StringValue), client,
                                                                                    operationsFromProduct, rollbackDocument, log);
                            }
                            else
                            {
                                ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformInfo.StringValue), client, betDocument.Id,
                                    operationsFromProduct, winDocument, WebApiApplication.DbLogger);
                            }
                        }
                        else if(betStatus == (int)BetDocumentStates.Deleted)
                        {
                            var rollbackDocument = documents.First(x => x.OperationTypeId == (int)OperationTypes.BetRollback);
                            ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformInfo.StringValue), client,
                                                                                operationsFromProduct, rollbackDocument, log);
                        }
                    }

                    return new ApiResponseBase { ResponseObject = betId };
                }
            }
            catch (FaultException<BllFnErrorType> e)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(e.Detail));
                return new ApiResponseBase { ResponseCode = e.Detail.Id };
            }
            catch (Exception e)
            {
                WebApiApplication.DbLogger.Error(e);
                return new ApiResponseBase { ResponseCode = Errors.GeneralException };
            }
        }

        private static ApiResponseBase SettleBet(InternetBetModel input, SessionIdentity identity, ILog log)
        {
            try
            {
                using (var documentBl = new DocumentBll(identity, log))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var betDocument = documentBl.GetDocumentById(input.BetDocumentId);
                        if (betDocument == null || betDocument.State != (int)BetDocumentStates.Uncalculated)
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongDocumentId);
                        var client = CacheManager.GetClientById(betDocument.ClientId.Value);

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = betDocument.CurrencyId,
                            RoundId = betDocument.RoundId,
                            GameProviderId = betDocument.GameProviderId.Value,
                            OperationTypeId = (int)OperationTypes.Win,
                            ProductId = betDocument.ProductId,
                            TransactionId = "M_" + betDocument.Id.ToString(),
                            CreditTransactionId = betDocument.Id,
                            Info = string.Empty,
                            State = input.WinAmount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost,
                            OperationItems = new List<OperationItemFromProduct>
                        {
                            new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.WinAmount > 0 ? input.WinAmount.Value : 0
                            }
                        }
                        };

                        var documents = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);

                        return new ApiResponseBase { ResponseObject = new { input.BetDocumentId, input.WinAmount, documents[0].State } };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> e)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(e.Detail));
                return new ApiResponseBase { ResponseCode = e.Detail.Id };
            }
            catch (Exception e)
            {
                WebApiApplication.DbLogger.Error(e);
                return new ApiResponseBase { ResponseCode = Errors.GeneralException };
            }
        }


        private static ApiResponseBase GetReportByClientsGamesPaging(ApiFilterClientGame input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                var filter = input.MapToFilterClientGame(identity.TimeZone);
                var games = reportBl.GetClientGamePagedModel(filter, string.Empty, true);

                return new ApiResponseBase
                {
                    ResponseObject = games
                };
            }
        }

        private static ApiResponseBase GetDuplicateClients(ApiFilterfnDuplicateClient input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                var pageModel = reportBl.GetDuplicateClients(input.MapToFilterDuplicateClient(identity.TimeZone));

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        pageModel.Count,
                        Entities = pageModel.Entities.Select(x=> new
                        {
                            x.DuplicatedClientId,
                            x.DuplicatedData,
                            MatchDate = x.LastUpdateTime.GetGMTDateFromUTC(identity.TimeZone)
                        }).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase GetAffiliateCorrections(ApiFilterAffiliateCorrection input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                var pageModel = reportBl.GetAffiliateCorrections(input.MapToFilterAffiliateCorrection(identity.TimeZone));

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        pageModel.Count,
                        Entities = pageModel.Entities.Select(x => new
                        {
                            x.Id,
                            x.PartnerId,
                            x.AffiliateId,
                            x.FirstName,
                            x.LastName,
                            x.Amount,
                            x.CurrencyId,
                            x.Creator,
                            x.OperationTypeName,
                            x.CreatorFirstName,
                            x.CreatorLastName,
                            x.DocumentTypeId,
                            x.ClientId,
                            x.ClientFirstName,
                            x.ClientLastName,
                            CreationTime = x.CreationTime.GetGMTDateFromUTC(identity.TimeZone),
                            LastUpdateTime = x.CreationTime.GetGMTDateFromUTC(identity.TimeZone)
                        }).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase GetReportByPopupStatistics(ApiFilterReportByPopupStatistics input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                var pageModel = reportBl.GetReportByPopupStatistics(input.MapToFilterReportByPopupStatistics(identity.TimeZone));

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        pageModel.Count,
                        Entities = pageModel.Entities.Select(x => new
                        {
                            PopupId = x.Id,
                            x.PartnerId,
                            x.NickName,
                            x.Type,
                            x.DeviceType,
                            x.State,
                            CreationTime = x.CreationTime.GetGMTDateFromUTC(identity.TimeZone),
                            LastUpdateTime = x.LastUpdateTime.GetGMTDateFromUTC(identity.TimeZone),
                            x.Viewed,
                            x.Closed,
                            x.Redirected
                        }).ToList()
                    }
                };
            }
        }
        private static ApiResponseBase ExportReportByPopupStatistics(ApiFilterReportByPopupStatistics input, SessionIdentity identity, ILog log)
        {
            using (var reportBl = new ReportBll(identity, log, 120))
            {
                var result = reportBl.ExportReportByPopupStatistics(input.MapToFilterReportByPopupStatistics(identity.TimeZone)).Entities.ToList();
                var fileName = "ExportReportByPopupStatistics.csv";
                var fileAbsPath = reportBl.ExportToCSV(fileName, result, input.FromDate, input.ToDate, identity.TimeZone, input.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

    }
}