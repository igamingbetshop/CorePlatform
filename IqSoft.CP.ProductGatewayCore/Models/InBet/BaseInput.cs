using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.InBet
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "session")]
        public string Session { get; set; }

        [JsonProperty(PropertyName = "minus")]
        public decimal? Minus { get; set; } //bet

        [JsonProperty(PropertyName = "plus")]
        public decimal? Plus { get; set; } //win

        [JsonProperty(PropertyName = "trx_id")]
        public string Trx_id { get; set; }

        [JsonProperty(PropertyName = "sign")]
        public string Sign { get; set; }

		[JsonProperty(PropertyName = "retry")]
		public string Retry { get; set; }

		[JsonProperty(PropertyName = "tag")]
        public Tag Tag { get; set; }
    }

    public class Tag
    {
        [JsonProperty(PropertyName = "game")]
        public string Game { get; set; }

        [JsonProperty(PropertyName = "game_id")]
        public string Game_id { get; set; }

        [JsonProperty(PropertyName = "game_uuid")]
        public string Game_uuid { get; set; }

        [JsonProperty(PropertyName = "lines")]
        public int Lines { get; set; }

        [JsonProperty(PropertyName = "bet")]
        public int Bet { get; set; }

        [JsonProperty(PropertyName = "denomination")]
        public int Denomination { get; set; }

		[JsonProperty(PropertyName = "bets")]
		public int[] Bets { get; set; }

		[JsonProperty(PropertyName = "result")]
        public int[] Result { get; set; }

        [JsonProperty(PropertyName = "round")]
        public string Round { get; set; }
    }

}