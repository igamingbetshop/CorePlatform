using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.PaymentIQ
{
    public class AuthTokenOutput
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }

        [JsonProperty(PropertyName = "jti")]
        public string Jti { get; set; }
    }
}