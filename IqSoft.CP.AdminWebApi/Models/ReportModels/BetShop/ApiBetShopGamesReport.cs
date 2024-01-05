using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop
{
    public class ApiBetShopGamesReport
    {
        public decimal TotalBetAmount { get; set; }

        public decimal TotalWinAmount { get; set; }
        public decimal TotalOriginalBetAmount { get; set; }

        public decimal TotalOriginalWinAmount { get; set; }

        public decimal TotalProfit { get; set; }

        public int TotalBetCount { get; set; }

        public List<ApiBetShopGame> Entities { get; set; }
    }
}