using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Instapay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "request_type")]
        public string RequestType { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "inst_transaction_id")]
        public string InstTransactionId { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "error_message")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "reject_reason")]
        public string RejectRreason { get; set; }

        [JsonProperty(PropertyName = "date_time")]
        public string DateTime { get; set; }

        [JsonProperty(PropertyName = "hash")]
        public string Hash { get; set; }

    }
}