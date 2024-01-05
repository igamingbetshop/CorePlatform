using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Report
{
    public class BetShopGames
    {
        public decimal? TotalBetAmount { get; set; }

        public decimal? TotalWinAmount { get; set; }

        public int TotalBetCount { get; set; }

        public List<BetShopGame> Entities { get; set; }
    }
}
