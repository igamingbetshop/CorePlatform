using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class Authentication
    {

        [JsonProperty(PropertyName = "Token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "AllFieldsComplete")]
        public bool AllFieldsComplete { get; set; }

        [JsonProperty(PropertyName = "errordescription")]
        public string Errordescription { get; set; }
    }

    public class AuthenticationOutput
    {
        [JsonProperty(PropertyName = "Authentication")]
        public Authentication Authentication { get; set; }
    }
}
