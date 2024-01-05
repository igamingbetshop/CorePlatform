using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.VisionaryiGaming
{
    public class BalanceOutput
    {
        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; } = "OK";

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = "OK";
    }
}