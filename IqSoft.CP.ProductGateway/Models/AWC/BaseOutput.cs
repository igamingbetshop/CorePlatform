
using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.AWC
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "balanceTs")]
        public string Timestamp { get; set; }
    }
}