using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Skrill
{
   public class SkrillRequestStatus
    {
        [JsonProperty(PropertyName = "pay_to_email")]
        public string PayToEmail { get; set; }

        [JsonProperty(PropertyName = "pay_from_email")]
        public string PayFromEmail { get; set; }

        [JsonProperty(PropertyName = "merchant_id")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "customer_id")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "mb_transaction_id")]
        public string mb_transaction_id { get; set; }

        [JsonProperty(PropertyName = "mb_amount")]
        public string TotalAmount { get; set; }

        [JsonProperty(PropertyName = "mb_currency")]
        public string CurrencyOfTotalAmount { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "failed_reason_code")]
        public string failed_reason_code { get; set; }

        [JsonProperty(PropertyName = "md5sig")]
        public string Signature { get; set; }

       [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

       [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }
}
