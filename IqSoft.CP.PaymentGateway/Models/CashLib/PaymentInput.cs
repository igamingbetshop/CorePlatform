using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.CashLib
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "transaction_id")]
        public int MerchantTransactionId { get; set; }

        [JsonProperty(PropertyName = "transaction_reference")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "purchase_amount")]
        public int Amount { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "error_message")]
        public string ErrorMessage { get; set; }
    }
}