using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SolidGaming
{
    public class BetInput : BaseInput
    {
        [JsonProperty(PropertyName = "playerId")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "roundId")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "currencyCode")]
        public string Currency { get; set; }

		[JsonProperty(PropertyName = "bet")]
        public Transaction bet { get; set; }

        [JsonProperty(PropertyName = "win")]
        public Transaction win { get; set; }

        [JsonProperty(PropertyName = "roundEnded")]
        public bool RoundEnded { get; set; }       
    }

    public class Transaction
    {
        [JsonProperty(PropertyName = "transactionId")]
        public string transactionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal amount { get; set; }

        [JsonProperty(PropertyName = "jackpotAmount")]
        public decimal jackpotAmount { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public long timestamp { get; set; }
    }
}