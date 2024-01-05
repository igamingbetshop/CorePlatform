using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Racebook
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "player")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "extraInfo")]
        public string ExtraInfo { get; set; }

        [JsonProperty(PropertyName = "hash")]
        public string Hash { get; set; }
    }
}