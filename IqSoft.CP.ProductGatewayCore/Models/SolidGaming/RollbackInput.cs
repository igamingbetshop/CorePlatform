using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SolidGaming
{
    public class RollbackInput
    {
        [JsonProperty(PropertyName = "playerId")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "gameCode")]
        public string GameCode { get; set; }

        [JsonProperty(PropertyName = "roundId")]
        public string RoundId { get; set; }
    }
}