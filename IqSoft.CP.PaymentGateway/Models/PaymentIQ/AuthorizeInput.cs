using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.PaymentIQ
{
    public class AuthorizeInput
    {
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "txAmount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "txAmountCy")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "txId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "txTypeId")]
        public string TxTypeId { get; set; }

        [JsonProperty(PropertyName = "txName")]
        public string TxName { get; set; }

        [JsonProperty(PropertyName = "maskedAccount")]
        public string MaskedAccount { get; set; }

        [JsonProperty(PropertyName = "accountHolder")]
        public string AccountHolder { get; set; }

        [JsonProperty(PropertyName = "provider")]
        public string Provider { get; set; }

        [JsonProperty(PropertyName = "pspService")]
        public string PSPService { get; set; }

        [JsonProperty(PropertyName = "pspRefId")]
        public string PSPRefId { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public AttributeModel Attributes { get; set; }
    }
}