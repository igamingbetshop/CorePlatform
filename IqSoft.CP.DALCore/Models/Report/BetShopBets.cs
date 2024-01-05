namespace IqSoft.CP.DAL.Models.Report
{
    public class BetShopBets : PagedModel<fnBetShopBet>
    {
        public decimal? TotalBetAmount { get; set; }

        public decimal? TotalWinAmount { get; set; }

        public decimal? TotalProfit { get; set; }
    }
}
