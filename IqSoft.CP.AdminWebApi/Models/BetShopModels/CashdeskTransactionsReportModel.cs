using System.Collections.Generic;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.AdminWebApi.Models.BetShopModels
{
    public class CashdeskTransactionsReportModel : PagedModel<CashDeskTransactionModel>
    {
        public List<CashdeskTransactionsReportTotalsModel> Totals { get; set; }
    }

    public class CashdeskTransactionsReportTotalsModel
    {
        public int OperationTypeId { get; set; }
        public string OperationTypeName { get; set; }
        public decimal? Total { get; set; }
        public string CurrencyId { get; set; }
    }
}