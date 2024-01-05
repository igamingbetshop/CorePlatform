using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TVBet
{
    public class BaseOutput : BaseModel
    {
        [JsonProperty(PropertyName = "si", NullValueHandling = NullValueHandling.Ignore)]
        public string Signature { get; set; }

    }
    public class BaseModel
    {
        [JsonProperty(PropertyName = "ti", NullValueHandling = NullValueHandling.Ignore)]
        public long UnixTime { get; set; }

        [JsonProperty(PropertyName = "sc", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsSuccess { get; set; }

        [JsonProperty(PropertyName = "cd", NullValueHandling = NullValueHandling.Ignore)]
        public int ResultCode { get; set; }

        [JsonProperty(PropertyName = "er", NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorDescription { get; set; }

        [JsonProperty(PropertyName = "val", NullValueHandling = NullValueHandling.Ignore)]
        public ClientData ClientVal { get; set; }
    }

    public class ClientData
    {
        [JsonProperty(PropertyName = "tid", NullValueHandling = NullValueHandling.Ignore)]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "dt", NullValueHandling = NullValueHandling.Ignore)]
        public long? TransactionTime { get; set; }

        [JsonProperty(PropertyName = "bid", NullValueHandling = NullValueHandling.Ignore)]
        public long? TransactionExternalId { get; set; }

        [JsonProperty(PropertyName = "tt", NullValueHandling = NullValueHandling.Ignore)]
        public int? TransactionType{ get; set; }

        [JsonProperty(PropertyName = "uid", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "sm", NullValueHandling = NullValueHandling.Ignore)]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "cc", NullValueHandling = NullValueHandling.Ignore)]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "to", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "ts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTest { get; set; }

        [JsonProperty(PropertyName = "bl", NullValueHandling = NullValueHandling.Ignore)]
        public string Balance { get; set; }
    }
}