using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.PayTrust88
{
    class PayoutRequestOutput
    {
        [JsonProperty(PropertyName = "payout")]
        public int ExternalTransactionId { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "item_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "item_description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string CurrencyId { get; set; }

        [JsonProperty(PropertyName = "bank_code")]
        public string BankCode { get; set; }

        [JsonProperty(PropertyName = "bank_name")]
        public string BankName { get; set; }

        [JsonProperty(PropertyName = "iban")]
        public string Iban { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }
    }
}
