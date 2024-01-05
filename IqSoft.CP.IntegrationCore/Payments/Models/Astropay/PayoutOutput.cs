using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Astropay
{
    public class PayoutOutput
    {

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "merchant_cashout_id")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "cashout_id")]
        public string ExternalId { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string ErrorDescription { get; set; }
    }
}