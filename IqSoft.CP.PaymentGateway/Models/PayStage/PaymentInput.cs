using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.PayStage
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "transaction_number")]
        public string TransactionNumber { get; set; }

        [JsonProperty(PropertyName = "reference_no")]
        public string ReferenceNo { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "details")]
        public Details Details { get; set; }
    }

    public class Details
    {
        [JsonProperty(PropertyName = "credit_amount")]
        public int CreditAmount { get; set; }

        [JsonProperty(PropertyName = "credit_currency")]
        public string CreditCurrency { get; set; }

        [JsonProperty(PropertyName = "debit_currency")]
        public string DebitCurrency { get; set; }

        [JsonProperty(PropertyName = "debit_amount")]
        public int DebitAmount { get; set; }

        [JsonProperty(PropertyName = "exchange_rate")] 
        public int ExchangeRate { get; set; }

        [JsonProperty(PropertyName = "fee")]
        public int Fee { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "total_amount")]
        public int TotalAmount { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}