using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.PaySec
{
    public class PayoutResultInput
    {
        [JsonProperty(PropertyName = "transactionReference")]
        public string TransactionReference { get; set; }

        [JsonProperty(PropertyName = "cartId")]
        public string CartId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "orderAmount")]
        public string OrderAmount { get; set; }

        [JsonProperty(PropertyName = "orderTime")]
        public string OrderTime { get; set; }

        [JsonProperty(PropertyName = "completedTime")]
        public string CompletedTime { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "statusMessage")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }
    }
}