using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TomHorn
{
    public class BalanceInput
    {
        [JsonProperty(PropertyName = "partnerID")]
        public string OperatorId { get; set; }

        [JsonProperty(PropertyName = "sign")]
        public string Sign { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
        
        [JsonProperty(PropertyName = "sessionID")]
        public long? SessionId { get; set; }

        [JsonProperty(PropertyName = "gameModule")]
        public string GameModule { get; set; }

        [JsonProperty(PropertyName = "type")]
        public int Type { get; set; }
    }
}