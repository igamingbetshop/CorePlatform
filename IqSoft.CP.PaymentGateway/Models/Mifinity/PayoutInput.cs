using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Mifinity
{
    public class PayoutInput
    {
        [JsonProperty(PropertyName = "money")]
        public Money MoneyDetails { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "destination")]
        public string Destination { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "transactionStatus")]
        public int TransactionStatus { get; set; }

        [JsonProperty(PropertyName = "transactionStatusDescription")]
        public string TransactionStatusDescription { get; set; }

        [JsonProperty(PropertyName = "traceId")]
        public string TraceId { get; set; }

        [JsonProperty(PropertyName = "transactionReference")]
        public string TransactionReference { get; set; }

        [JsonProperty(PropertyName = "eventTypeDescription")]
        public string EventTypeDescription { get; set; }

        [JsonProperty(PropertyName = "paymentCategoryType")]
        public string PaymentCategoryType { get; set; }
    }

    public class Money
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "presentationAmount")]
        public string PresentationAmount { get; set; }
    }
}