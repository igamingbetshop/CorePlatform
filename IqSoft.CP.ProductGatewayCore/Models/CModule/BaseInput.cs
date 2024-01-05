using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.CModule
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "sign")]
        public string sign { get; set; }

        [JsonProperty(PropertyName = "session")]
        public string session { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string currency { get; set; }

        [JsonProperty(PropertyName = "game_id")]
        public int? game_id { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public int? amount { get; set; }

        [JsonProperty(PropertyName = "trx_id")]
        public string trx_id { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public int? balance { get; set; }

        [JsonProperty(PropertyName = "meta")]
        public Meta meta { get; set; }

        [JsonProperty(PropertyName = "id_group")]
        public string id_group { get; set; }

        [JsonProperty(PropertyName = "id_player")]
        public string id_player { get; set; }

        [JsonProperty(PropertyName = "round_id")]
        public string round_id { get; set; }
    }

    public class Meta
    {
        [JsonProperty(PropertyName = "tag")]
        public Tag tag { get; set; }
    }

    public class Tag
    {
        [JsonProperty(PropertyName = "game")]
        public string game { get; set; }
        
        [JsonProperty(PropertyName = "game_id")]
        public int game_id { get; set; }

        [JsonProperty(PropertyName = "game_uuid")]
        public string game_uuid { get; set; }

        [JsonProperty(PropertyName = "lines")]
        public int lines { get; set; }

        [JsonProperty(PropertyName = "bet")]
        public int bet { get; set; }

        [JsonProperty(PropertyName = "denomination")]
        public int denomination { get; set; }

        [JsonProperty(PropertyName = "round_id")]
        public int round_id { get; set; }
    }
}