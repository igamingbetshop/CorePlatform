using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.JackpotGaming
{
    public class CloseRoundInput
    {
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "gameId")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "reference")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "roundClose")]
        public bool RoundClose { get; set; }
    }
}