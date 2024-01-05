using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop
{
    public class ApiBetShopsReport
    {
        public decimal TotalBetAmount { get; set; }

        public decimal TotalWinAmount { get; set; }

		public decimal TotalProfit { get; set; }

		public List<ApiBetShopReport> Entities { get; set; }
    }
}