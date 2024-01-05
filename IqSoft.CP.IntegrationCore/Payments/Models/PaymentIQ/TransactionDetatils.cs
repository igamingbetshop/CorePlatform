using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.PaymentIQ
{
    public class TransactionDetatils
    {
        [JsonProperty(PropertyName = "merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "txRefId")]
        public string TxRefId { get; set; }

        [JsonProperty(PropertyName = "txTypeInt")]
        public string TxTypeInt { get; set; }

        [JsonProperty(PropertyName = "txType")]
        public string TxType { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "stateInt")]
        public string StateInt { get; set; }

        [JsonProperty(PropertyName = "statusCode")]
        public string StatusCode { get; set; }

        [JsonProperty(PropertyName = "merchantUserId")]
        public string MerchantUserId { get; set; }

        [JsonProperty(PropertyName = "maskedUserAccount")]
        public string MaskedUserAccount { get; set; }

        [JsonProperty(PropertyName = "userPspAccountId")]
        public string UserPspAccountId { get; set; }

        [JsonProperty(PropertyName = "merchantTxId")]
        public string MerchantTxId { get; set; }
    }
}