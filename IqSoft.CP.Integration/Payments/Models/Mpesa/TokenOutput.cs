using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Mpesa
{
    public class TokenOutput
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
    }
}
