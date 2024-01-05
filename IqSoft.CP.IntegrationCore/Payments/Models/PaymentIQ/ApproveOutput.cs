using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.PaymentIQ
{
    public class ApproveOutput
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "statusCode")]
        public string StatusCode { get; set; }

        [JsonProperty(PropertyName = "pspStatusCode")]
        public string PspStatusCode { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "created")]
        public string Created { get; set; }

        [JsonProperty(PropertyName = "lastUpdated")]
        public string LastUpdated { get; set; }
    }
}