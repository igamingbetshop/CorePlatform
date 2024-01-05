using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Skrill
{
    public class SkrillRequestStatus
    {
        [JsonProperty(PropertyName = "pay_to_email")]
        public string pay_to_email { get; set; }

        [JsonProperty(PropertyName = "pay_from_email")]
        public string pay_from_email { get; set; }

        [JsonProperty(PropertyName = "merchant_id")]
        public string merchant_id { get; set; }

        [JsonProperty(PropertyName = "customer_id")]
        public string customer_id { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public long transaction_id { get; set; }

        [JsonProperty(PropertyName = "mb_transaction_id")]
        public string mb_transaction_id { get; set; }

        [JsonProperty(PropertyName = "mb_amount")]
        public string mb_amount { get; set; }

        [JsonProperty(PropertyName = "mb_currency")]
        public string mb_currency { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "failed_reason_code")]
        public string failed_reason_code { get; set; }

        [JsonProperty(PropertyName = "md5sig")]
        public string Md5Sig { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }
}