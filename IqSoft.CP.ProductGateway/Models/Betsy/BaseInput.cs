using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Betsy
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }
    }
}