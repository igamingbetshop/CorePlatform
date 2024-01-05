using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    public class AuthenticationInput : BaseInput
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }
}