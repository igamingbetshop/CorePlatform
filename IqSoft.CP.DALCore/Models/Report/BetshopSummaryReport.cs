using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Report
{
    public class BetshopSummaryReport
    {
        public int BetShopId { get; set; }

        public string BetShopName { get; set; }

        public string CurrencyId { get; set; }

        public IEnumerable<BetshopCategoryReport> Categorys { get; set; }
    }

    public class BetshopCategoryReport
    {
        public string ProductName { get; set; }

        public int ProductId { get; set; }

        public decimal BetAmount { get; set; }

        public decimal WinAmount { get; set; }
    }
}
