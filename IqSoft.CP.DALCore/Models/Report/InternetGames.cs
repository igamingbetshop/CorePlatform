using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Report
{
    public class InternetGames
    {
        public decimal? TotalBetAmount { get; set; }

        public decimal? TotalWinAmount { get; set; }

        public int TotalBetCount { get; set; }

        public decimal TotalSupplierFee { get; set; }

        public List<InternetGame> Entities { get; set; }
    }
}
