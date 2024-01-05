using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.JackpotGaming
{
    public class BalanceInput
    {
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }
}