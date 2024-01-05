using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.NodaPay
{
    public class PaymentRequest
    {
        [JsonProperty(PropertyName = "paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        
        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "merchantPaymentId")]
        public string MerchantPaymentId { get; set; }

        [JsonProperty(PropertyName = "reference")]
        public string Reference { get; set; }
    }
}