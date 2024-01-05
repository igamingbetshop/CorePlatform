using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Transact365
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "trans_id")]
        public string TransId { get; set; }

        [JsonProperty(PropertyName = "auth_version")]
        public decimal Version { get; set; }

        [JsonProperty(PropertyName = "auth_key")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "auth_timestamp")]
        public int Timestamp { get; set; }

        [JsonProperty(PropertyName = "auth_signature")]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty(PropertyName = "statusMessage")]
        public string StatusMessage { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }
    }
}