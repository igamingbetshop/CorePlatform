using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Astropay
{
    public class PayoutInput
    {
        [JsonProperty(PropertyName = "cashout_external_id")]
        public string ExternalId { get; set; }

        [JsonProperty(PropertyName = "merchant_cashout_id")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "cashout_user_id")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "end_status_date")]
        public string EndStatusDate { get; set; }
    }
}