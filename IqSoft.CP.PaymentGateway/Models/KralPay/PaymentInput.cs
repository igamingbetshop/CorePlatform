using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.KralPay
{
    public class PaymentInput : PaymentInputBase
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "description ")]
        public string Description { get; set; }
    }
}