using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.EZeeWallet
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "transaction_id")]
        public long MerchantTransactionId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }
    }
}