using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Chapa
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "event")]
        public string Event { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "charge")]
        public decimal Charge { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "reference")]
        public string Reference { get; set; }

        [JsonProperty(PropertyName = "chapa_reference")]
        public string ChapaReference { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "tx_ref")]
        public string TxRef { get; set; }

        [JsonProperty(PropertyName = "payment_method")]
        public string PaymentMethod { get; set; }
    }
}