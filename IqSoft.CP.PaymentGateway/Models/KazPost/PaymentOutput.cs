using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.KazPost
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "result")]
        public Result Result { get; set; }

        [JsonProperty(PropertyName = "errors")]
        public ErrorOutput Errors { get; set; }
    }

    public class ErrorOutput
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "non_field_errors")]
        public string Description { get; set; }
    }

    public class Result
    {
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string ExternalTransactionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "refund_amount")]
        public decimal RefundAmount { get; set; }

        [JsonProperty(PropertyName = "used_amount")]
        public decimal UsedAmount { get; set; }

        [JsonProperty(PropertyName = "date_created")]
        public string DateCreated { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }
}