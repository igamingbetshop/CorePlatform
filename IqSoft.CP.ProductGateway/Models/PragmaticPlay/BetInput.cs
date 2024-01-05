namespace IqSoft.CP.ProductGateway.Models.PragmaticPlay
{
    public class BetInput : BaseInput
    {
        public string roundId { get; set; }
        public decimal? amount { get; set; }
        public string reference { get; set; }
        public string timestamp { get; set; }
        public string roundDetails { get; set; }
        public string platform { get; set; }
        public string bonusCode { get; set; }
        public string jackpotId { get; set; }
        public string promoCampaignType { get; set; }
        public string promoCampaignID { get; set; }
        public decimal? promoWinAmount { get; set; }
        public string promoWinReference { get; set; }
        public string campaignType { get; set; }
        public string campaignId { get; set; }
        public string currency { get; set; }
    }
}