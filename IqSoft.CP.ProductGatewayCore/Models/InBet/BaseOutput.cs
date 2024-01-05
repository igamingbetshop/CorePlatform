using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.InBet
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal? Balance { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }
}