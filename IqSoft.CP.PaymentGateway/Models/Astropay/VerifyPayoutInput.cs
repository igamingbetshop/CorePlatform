using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Astropay
{
    public class VerifyPayoutInput
    {
        [JsonProperty(PropertyName = "cashout_external_id")]
        public string ExternalId { get; set; }

        [JsonProperty(PropertyName = "merchant_cashout_id")]
        public string MerchantRequestId { get; set; }

        [JsonProperty(PropertyName = "merchant_cashout_user_id")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }
    }

    public class VerifyPayoutOutput
    {
        [JsonProperty(PropertyName = "cashout_external_id")]
        public string ExternalId { get; set; }

        [JsonProperty(PropertyName = "approve")]
        public bool Approve { get; set; }
    }
}