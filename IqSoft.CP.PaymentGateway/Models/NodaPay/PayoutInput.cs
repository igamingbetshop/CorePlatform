using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.NodaPay
{
    public class PayoutInput
    {
        [JsonProperty(PropertyName = "id")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty(PropertyName = "iban")]
        public string Iban { get; set; }

        [JsonProperty(PropertyName = "beneficiaryName")]
        public string BeneficiaryName { get; set; }

        [JsonProperty(PropertyName = "beneficiaryRef")]
        public string BeneficiaryRef { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }
    }
}