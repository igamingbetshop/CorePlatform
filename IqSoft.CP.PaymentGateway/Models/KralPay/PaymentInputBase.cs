using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.KralPay
{
    public class PaymentInputBase
    {
        [JsonProperty(PropertyName = "sid")]
        public string SId { get; set; }

        [JsonProperty(PropertyName = "merchant_key")]
        public string MerchantKey { get; set; }

        [JsonProperty(PropertyName = "service")]
        public string Service { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "trx")]
        public string Trx { get; set; }
    }
}