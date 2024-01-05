using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ESport
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "auth_token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "client_id")]
        public int? OperatorId { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public int? PlayerId { get; set; }
    }
}