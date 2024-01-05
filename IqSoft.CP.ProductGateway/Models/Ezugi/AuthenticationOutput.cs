using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Ezugi
{
    public class AuthenticationOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "nickName")]
        public string NickName { get; set; }

        [JsonProperty(PropertyName = "playerTokenAtLaunch")]
        public string PlayerTokenAtLaunch { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "clientIP")]
        public string ClientIp { get; set; }
    }
}