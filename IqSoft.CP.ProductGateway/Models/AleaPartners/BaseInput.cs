using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.AleaPartners
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "userUuid")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string Token { get; set; }
    }
}