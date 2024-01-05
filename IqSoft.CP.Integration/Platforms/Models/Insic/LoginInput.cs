using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class LoginInput
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }
    }
}
