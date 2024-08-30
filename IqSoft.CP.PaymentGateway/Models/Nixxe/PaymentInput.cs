using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Nixxe
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "tracking_id")]
        public string TrackingId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "transaction_type")]
        public string TransactionType { get; set; }
    }
}