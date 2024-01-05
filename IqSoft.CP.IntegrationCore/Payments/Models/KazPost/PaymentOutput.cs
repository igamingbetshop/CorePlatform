using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.KazPost
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
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "payment")]
        public string TransactionId { get; set; }
    }

}
