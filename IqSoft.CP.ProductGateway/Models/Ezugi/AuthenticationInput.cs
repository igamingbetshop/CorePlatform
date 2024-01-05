using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Ezugi
{
    public class AuthenticationInput
    {
        [JsonProperty(PropertyName="operatorId")]
        public int OperatorId { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "platformId")]
        public int PlatformId { get; set; }
        
        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }
    }
}