using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Astropay
{
   public class PaymentOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "merchant_deposit_id")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "deposit_external_id")]
        public string ExternalId { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string ErrorDescription { get; set; }
    }
}
