using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.PaymentIQ
{
    public class AuthorizeOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "merchantTxId")]
        public string MerchantTxId { get; set; }

        [JsonProperty(PropertyName = "authCode")]
        public string AuthCode { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public AttributeModel Attributes { get; set; }
    }
}