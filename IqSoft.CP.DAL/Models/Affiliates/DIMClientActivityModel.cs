namespace IqSoft.CP.DAL.Models.Affiliates
{
    public class DIMClientActivityModel : ClientActivityModel
    {
        public decimal? PokerGrossRevenue { get; set; }
        public decimal? MahjongGrossRevenue { get; set; }
        public decimal PokerBonusBetsAmount { get; set; }
        public decimal MahjongBonusBetsAmount { get; set; }
        public decimal PokerBonusWinsAmount { get; set; }
        public decimal MahjongBonusWinsAmount { get; set; }
        public decimal PokerTotalWinAmount { get; set; }
        public decimal MahjongTotalWinAmount { get; set; }
        public decimal TotalConvertedBonusAmount { get; set; }
        public int PaymentTransactions { get; set; }
        public int TotalTransactions { get; set; }
    }
}
