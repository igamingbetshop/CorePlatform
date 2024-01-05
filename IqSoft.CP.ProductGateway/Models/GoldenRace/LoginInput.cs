using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.GoldenRace
{
    public class LoginInput : BaseInput
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "clientPlatform")]
        public string ClientPlatform { get; set; }

        [JsonProperty(PropertyName = "clientIp")]
        public string ClientIp { get; set; }
    }
}