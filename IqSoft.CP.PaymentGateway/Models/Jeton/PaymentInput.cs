using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Jeton
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "orderId")]
        public long OrderId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}