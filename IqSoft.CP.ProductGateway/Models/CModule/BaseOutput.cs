using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.CModule
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "id_player")]
        public int? PlayerId { get; set; }

        [JsonProperty(PropertyName = "id_group")]
        public string GroupId { get; set; }

        [JsonProperty(PropertyName = "game_id")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public long Balance { get; set; }

        [JsonProperty(PropertyName = "cmd")]
        public string Command { get; set; }
    }
}