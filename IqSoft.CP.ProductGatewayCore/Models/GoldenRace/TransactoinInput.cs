using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.GoldenRace
{
    public class TransactoinInput : BaseInput
    {
        [JsonProperty(PropertyName = "playerId")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "gameCycle")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "gameCycleClosed")]
        public bool GameCycleClosed { get; set; }

        [JsonProperty(PropertyName = "transactionAmount")]
        public decimal TransactionAmount { get; set; }

        [JsonProperty(PropertyName = "transactionCategory")]
        public string TransactionCategory { get; set; }
    }
}