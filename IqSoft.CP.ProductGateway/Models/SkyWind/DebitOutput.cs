using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SkyWind
{
    public class DebitOutput :BaseOutput
    {
        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "trx_id")]
        public string BetId { get; set; }

        //[JsonProperty(PropertyName = "free_bet_count")]
        //public int FreeBetCount { get; set; }

    }
}