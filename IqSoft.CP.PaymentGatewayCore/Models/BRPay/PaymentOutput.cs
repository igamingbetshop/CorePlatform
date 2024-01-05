using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.BRPay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "sp_salt")]
        public string Salt { get; set; }

        [JsonProperty(PropertyName = "sp_status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "sp_sig")]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "sp_error_description ")]
        public string ErrorDescription { get; set; }
    }
}