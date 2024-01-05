using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Betsy
{
    public class BalanceInput : BaseInput
    {
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
    }
}