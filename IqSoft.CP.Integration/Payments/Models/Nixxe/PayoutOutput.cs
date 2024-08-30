using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Nixxe
{
    public class PayoutOutput : ErrorOutput
    {
        [JsonProperty(PropertyName = "transaction_status")]
        public string TransactionStatus { get; set; }
    }

    public class ErrorOutput
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "awsRequestId")]
        public string TransactionId { get; set; }
    }
}