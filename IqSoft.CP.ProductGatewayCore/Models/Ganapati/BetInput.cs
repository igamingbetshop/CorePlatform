using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.Ganapati
{
    public class BetInput
    {
        [JsonProperty(PropertyName = "playerId")]
        public string PlayerId { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string RefreshToken { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal? Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "game")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "gameRound")]
        public string GameRound { get; set; }

        [JsonProperty(PropertyName = "roundEnd")]
        public bool RoundEnd { get; set; }

        [JsonProperty(PropertyName = "extra")]
        public object ExtraInformation { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}