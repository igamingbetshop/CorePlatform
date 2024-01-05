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

        [JsonProperty(PropertyName = "attributes")]
        public AttributeModel Attributes { get; set; }
    }
}