using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.RelaxGaming
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "customerid")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "cashiertoken")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "gameref")]
        public string GameId { get; set; }
    }
}