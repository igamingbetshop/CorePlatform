using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TVBet
{
    public class BaseOutput : BaseModel
    {
        [JsonProperty(PropertyName = "si")]
        public string Signature { get; set; }

    }
    public class BaseModel
    {
        [JsonProperty(PropertyName = "ti")]
        public long UnixTime { get; set; }

        [JsonProperty(PropertyName = "sc")]
        public bool IsSuccess { get; set; }

        [JsonProperty(PropertyName = "cd")]
        public int ResultCode { get; set; }

        [JsonProperty(PropertyName = "er")]
        public string ErrorDescription { get; set; }

        [JsonProperty(PropertyName = "val")]
        public ClientData ClientVal { get; set; }
    }

    public class ClientData
    {
        [JsonProperty(PropertyName = "tid")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "dt")]
        public long? TransactionTime { get; set; }

        [JsonProperty(PropertyName = "bid")]
        public long? TransactionExternalId { get; set; }

        [JsonProperty(PropertyName = "tt")]
        public int? TransactionType{ get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "sm")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "cc")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "to")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "ts")]
        public bool? IsTest { get; set; }

        [JsonProperty(PropertyName = "bl")]
        public string Balance { get; set; }
    }
}