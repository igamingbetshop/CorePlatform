using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SkyCity
{
    public class BetInput : BaseInput
    {
        [JsonProperty(PropertyName = "roundid")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "gametype")]
        public string GameType { get; set; }

        [JsonProperty(PropertyName = "tableid")]
        public string TableId { get; set; }

        [JsonProperty(PropertyName = "bet")]
        public Bet Bet { get; set; }
    }

    public class Bet
    {
        [JsonProperty(PropertyName = "tid")]
        public long ExternalTransactionId { get; set; }

        [JsonProperty(PropertyName = "transactionid")]
        public long TransactionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "winamount")]
        public decimal WinAmount { get; set; }
    }
}