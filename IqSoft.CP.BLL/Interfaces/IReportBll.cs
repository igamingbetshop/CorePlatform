using System;
using System.Collections.Generic;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Report;
using IqSoft.CP.DAL.Models.Dashboard;
using IqSoft.CP.DAL.Models.RealTime;
using IqSoft.CP.DAL.Models.PlayersDashboard;
using IqSoft.CP.DAL.Filters.Reporting;
using IqSoft.CP.DAL;
using IqSoft.CP.DataWarehouse.Filters;
using IqSoft.CP.DataWarehouse;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.BLL.Interfaces
{
    public interface IReportBll : IBaseBll
    {
        List<string> GetObjectHistoryElementById(long objectHistoryId);

        List<ShiftInfo> GetShifts(DateTime startTime, DateTime endTime, int cashDeskId, int? cashierId);

        List<fnCashDeskTransaction> GetCashDeskTransactions(FilterCashDeskTransaction filter, string languageId);

        CashdeskTransactionsReport GetCashDeskTransactionsPage(FilterCashDeskTransaction filter);

        PagedModel<fnCorrection> GetClientCorrections(FilterCorrection filter, bool checkPermission);

        BetShops GetReportByBetShops(FilterBetShopBet filter);

		BetShopReconingOutput GetBetShopReconingsPage(FilterfnBetShopReconing filter);

        RealTimeInfo GetOnlineClients(FilterRealTime input);

        ClientReport GetClientsInfoList(FilterfnClientDashboard filter);

        BetsInfo GetBetsInfoForDashboard(FilterDashboard filter);

        List<PaymentRequestsInfo> GetPaymentRequestsForDashboard(FilterDashboard filter, int type);

        PlayersInfo GetPlayersInfoForDashboard(FilterDashboard filter);

        ProvidersBetsInfo GetProviderBetsForDashboard(FilterDashboard filter);

        List<ObjectChangeHistoryItem> GetObjectChangeHistory(int objectTypeId, int objectId);

        #region Reporting

        #region BetShop Reports

        DataWarehouse.Models.BetShopBets GetBetshopBetsPagedModel(FilterBetShopBet filter, string currencyId, string permission, bool checkPermission);

        List<fnReportByBetShopOperation> GetReportByBetShopPayments(FilterReportByBetShopPayment filter);

        fnBetShopBet GetBetByBarcode(int cashDeskId, long barcode);

        #endregion

        #region InternetReports

        DataWarehouse.Models.InternetBetsReport GetInternetBetsPagedModel(FilterInternetBet filter, string currencyId, bool checkPermission);

        DataWarehouse.Models.InternetBetsReport GetBetsForWebSite(FilterWebSiteBet filter);


		InternetBetsByClientReport GetInternetBetsByClientPagedModel(FilterInternetBet filter);
        
        #endregion

        #region Business Intelligence Reports

        List<ReportByProvidersElement> GetReportByProviders(FilterReportByProvider filter);

        List<fnReportByProduct> GetReportByProducts(FilterReportByProduct filter);
        
        #endregion

        #region Business Audit

        PagedModel<fnActionLog> GetReportByActionLogPaging(FilterReportByActionLog filter, bool checkPermission);

        #endregion

        #region Accounting Reports

        PartnerPaymentsSummaryReport GetPartnerPaymentsSummaryReport(FilterPartnerPaymentsSummary filter);

        #endregion

        #endregion        

        List<fnInternetBet> ExportInternetBet(FilterInternetBet filter);

        List<fnReportByProduct> ExportProducts(FilterReportByProduct filter);

        List<BetShopReport> ExportBetShops(FilterBetShopBet filter);

        List<ReportByProvidersElement> ExportProviders(FilterReportByProvider filter);

        List<fnBetShopReconing> ExportBetShopReconings(FilterfnBetShopReconing filter);

    //    List<InternetBetByClient> ExportInternetBetsByClient(FilterInternetBet filter);
        
        List<fnBetShopBet> ExportBetShopBets(FilterBetShopBet filter);

        List<fnReportByBetShopOperation> ExportByBetShopPayments(FilterReportByBetShopPayment filter);

        List<BetshopSummaryReport> ExportBetshopSummary(FilterBetShopBet filter);

        List<fnActionLog> ExportByActionLogs(FilterReportByActionLog filter);

        List<fnCorrection> ExportClientCorrections(FilterCorrection filter);

        List<fnAccount> ExportClientAccounts(FilterfnAccount filter);

        List<ApiClientInfo> ExportClientsInfoList(FilterfnClientDashboard filter);
    }
}
