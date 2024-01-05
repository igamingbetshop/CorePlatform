using System.Collections.Generic;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Models.Report
{
    public class CashdeskTransactionsReport : PagedModel<fnCashDeskTransaction>
    {
        public List<CashdeskTransactionsReportTotals> Totals { get; set; }
    }

    public class CashdeskTransactionsReportTotals
    {
        public int OperationTypeId { get; set; }
        public string OperationTypeName { get; set; }
        public decimal? Total { get; set; }
        public string CurrencyId { get; set; }
    }
}
