using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ISoftBet
{
    public class ReportOutput
    {
        [JsonProperty(PropertyName = "session")]
        public int Session { get; set; }

        [JsonProperty(PropertyName = "rounds")]
        public int TotalRoundCount { get; set; }

        [JsonProperty(PropertyName = "transactions")]
        public int TotalTransactionsCount { get; set; }

        [JsonProperty(PropertyName = "bets")]
        public long TotalBetCount { get; set; }

        [JsonProperty(PropertyName = "bets_cancelled")]
        public int TotalCanceledBetCount { get; set; }

        [JsonProperty(PropertyName = "bets_amount")]
        public decimal TotalBetAmount { get; set; }

        [JsonProperty(PropertyName = "bets_amount_cancelled")]
        public decimal TotalCanceledBetAmount { get; set; }

        [JsonProperty(PropertyName = "wins")]
        public int TotalWinCount { get; set; }

        [JsonProperty(PropertyName = "wins_amount")]
        public decimal TotalWinAmount { get; set; }

        [JsonProperty(PropertyName = "fround_bets")]
        public int TotalFroundBetsCount { get; set; }

        [JsonProperty(PropertyName = "fround_wins_amount")]
        public decimal TotalFroundWinAmount { get; set; }

        [JsonProperty(PropertyName = "jp_contribution")]
        public decimal TotalJackpot { get; set; }

        [JsonProperty(PropertyName = "jp_wins")]
        public decimal TotalJackpotWinAmount { get; set; }
    }
}