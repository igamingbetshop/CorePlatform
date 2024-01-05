using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Models.NodaPay
{
    public class PayoutResponse
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "iban")]
        public string Iban { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(PropertyName = "beneficiaryName")]
        public string BeneficiaryName { get; set; }

        [JsonProperty(PropertyName = "beneficiaryRef")]
        public string BeneficiaryRef { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "Message")]
        public string Message { get; set; }
    }
}