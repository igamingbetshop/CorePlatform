using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.RelaxGaming
{
    public class AuthenticationInput
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "gameref")]
        public string GameId { get; set; }
    }
}