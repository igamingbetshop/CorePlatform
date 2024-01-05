using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.MGTCompliance
{
    public class TokenOutput : ErrorOutput
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

    }
}
