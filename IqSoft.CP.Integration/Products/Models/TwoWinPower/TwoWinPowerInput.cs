using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.TwoWinPower
{
    public class TwoWinPowerInput
    {
        [JsonProperty(PropertyName = "game_uuid")]
        public string game_uuid { get; set; }

        [JsonProperty(PropertyName = "player_id")]
        public string player_id { get; set; }

        [JsonProperty(PropertyName = "player_name")]
        public string player_name { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string currency { get; set; }

        [JsonProperty(PropertyName = "session_id")]
        public string session_id { get; set; }

        [JsonProperty(PropertyName = "return_url")]
        public string return_url { get; set; }

        /*[JsonProperty(PropertyName = "language")]
        public string language { get; set; }*/
    }
}
