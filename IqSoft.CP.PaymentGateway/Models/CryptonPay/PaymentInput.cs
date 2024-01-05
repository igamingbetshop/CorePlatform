using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.CryptonPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "merchantTransactionId")]
        public string MerchantTransactionId { get; set; }

        [JsonProperty(PropertyName = "merchantUserId")]
        public string MerchantUserId { get; set; }

        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "fee")]
        public string Fee { get; set; }

        [JsonProperty(PropertyName = "status")]
        public StatusModel Status { get; set; }
    }

    public class StatusModel
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}