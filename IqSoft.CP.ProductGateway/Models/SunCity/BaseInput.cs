using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SunCity
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "testmode")]
        public string TestMode { get; set; }

        [JsonProperty(PropertyName = "users")]
        public User[] Users { get; set; }
    }

    public class User
    {
        [JsonProperty(PropertyName = "authtoken")]
        public string AuthToken { get; set; }

        [JsonProperty(PropertyName = "userid")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "brandcode")]
        public string Brandcode { get; set; }

        [JsonProperty(PropertyName = "lang")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "cur")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "walletcode")]
        public string WalletCode { get; set; }
    }
}