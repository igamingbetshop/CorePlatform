using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Transact365
{
    public class AuthenticationInput
    {
        [JsonProperty(PropertyName = "auth_version")]
        public float Version { get; set; }

        [JsonProperty(PropertyName = "auth_key")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "auth_timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty(PropertyName = "auth_signature")]
        public string Signature { get; set; }
    }
}
