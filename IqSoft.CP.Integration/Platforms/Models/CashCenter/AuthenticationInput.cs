using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.EBC
{
    public class AuthenticationInput
    {
        [JsonProperty(PropertyName = "grant_type")]
        public string grant_type { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string username { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string password { get; set; }

        [JsonProperty(PropertyName = "client_id")]
        public string client_id { get; set; }
    }
}
