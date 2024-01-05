using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TurboGames
{
    public class BalanceInput : BaseInput
    {
        [JsonProperty(PropertyName = "userId")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }
}