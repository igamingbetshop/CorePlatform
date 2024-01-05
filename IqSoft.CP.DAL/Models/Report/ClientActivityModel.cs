namespace IqSoft.CP.DAL.Models.Report
{
    public class ClientActivityModel
    {
        public int BrandId { get; set; }
        public int CustomerId { get; set; }
        public string CurrencyId { get; set; }
        public string BTag { get; set; }
        public string ActivityDate { get; set; }
        public decimal? SportGrossRevenue { get; set; }
        public decimal? CasinoGrossRevenue { get; set; }
        public decimal? PokerGrossRevenue { get; set; }
        public decimal? MahjongGrossRevenue { get; set; }
        public decimal SportBonusBetsAmount { get; set; }
        public decimal CasinoBonusBetsAmount { get; set; }
        public decimal PokerBonusBetsAmount { get; set; }
        public decimal MahjongBonusBetsAmount { get; set; }
        public decimal SportBonusWinsAmount { get; set; }
        public decimal CasinoBonusWinsAmount { get; set; }
        public decimal PokerBonusWinsAmount { get; set; }
        public decimal MahjongBonusWinsAmount { get; set; }
        public decimal SportTotalWinAmount { get; set; }
        public decimal CasinoTotalWinAmount { get; set; }
        public decimal PokerTotalWinAmount { get; set; }
        public decimal MahjongTotalWinAmount { get; set; }
        public decimal Deposits { get; set; }
        public decimal Withdrawals { get; set; }
        public decimal TotalConvertedBonusAmount { get; set; }
        public int Category { get; set; }

        public int PaymentTransactions { get; set; }
        public int Transactions { get; set; }

    }
}
