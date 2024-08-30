using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.LiberSave
{
    public class PaymentEncryptedInput
    {
        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonProperty(PropertyName = "sign")]
        public string Sign { get; set; }
    }

    public class PaymentInput
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}