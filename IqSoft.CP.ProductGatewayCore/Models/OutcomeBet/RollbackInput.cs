using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.OutcomeBet
{
    public class RollbackInput
    {
        [JsonProperty(PropertyName = "callerId")]
        public int CallerId { get; set; }

        [JsonProperty(PropertyName = "playerName")]
        public string PlayerName { get; set; }

        [JsonProperty(PropertyName = "transactionRef")]
        public string TransactionRef { get; set; }

        [JsonProperty(PropertyName = "gameId")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "sessionAlternativeId")]
        public string SessionAlternativeId { get; set; }
    }
}