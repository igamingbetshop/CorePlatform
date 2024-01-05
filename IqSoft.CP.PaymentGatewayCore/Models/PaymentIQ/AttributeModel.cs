using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.PaymentIQ
{
    public class AttributeModel
    {
        [JsonProperty(PropertyName = "merchantTxId")]
        public long MerchantTransactionId { get; set; }
    }
}