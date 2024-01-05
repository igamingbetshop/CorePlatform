using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class LoginOutput
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }
}
