using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.BRPay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "sp_salt")]
        public string Salt { get; set; }

        [JsonProperty(PropertyName = "sp_status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "sp_payment_id")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "sp_redirect_url_type")]
        public string RedirectUrlType { get; set; }

        [JsonProperty(PropertyName = "sp_redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "sp_host2host")]
        public string Host2host { get; set; }

        [JsonProperty(PropertyName = "sp_sig")]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "sp_error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "sp_error_description")]
        public string ErrorDescription { get; set; }
    }
}
