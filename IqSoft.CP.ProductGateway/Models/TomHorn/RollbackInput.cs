using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TomHorn
{
    public class RollbackInput
    {
        [JsonProperty(PropertyName = "partnerID")]
        public string OperatorId { get; set; }

        [JsonProperty(PropertyName = "sign")]
        public string Sign { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "reference")]
        public long? TransactionId { get; set; }

        [JsonProperty(PropertyName = "sessionID")]
        public long? SessionId { get; set; }
    }
}