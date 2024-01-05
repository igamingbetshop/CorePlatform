using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class ResponseBase
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }
    }
}
