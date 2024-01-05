using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.CardToCard
{
    public class PayoutRequestOutput
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "msg")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "data")]
        public PaymentData Data { get; set; }
    }

    public class PaymentData
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "ref_id")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "request_id")]
        public string ExternalId { get; set; }


        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}
