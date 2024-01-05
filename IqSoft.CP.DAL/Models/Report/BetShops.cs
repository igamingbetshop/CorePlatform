using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Report
{
    public class BetShops
    {
        public decimal? TotalBetAmount { get; set; }

        public decimal? TotalWinAmount { get; set; }

		public decimal? TotalProfit { get; set; }

		public List<BetShopReport> Entities { get; set; }
    }
}
