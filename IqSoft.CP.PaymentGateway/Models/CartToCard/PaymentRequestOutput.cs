using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.CartToCard
{
    public class PaymentRequestOutput
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

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "last_verify")]
        public bool LastVerify { get; set; }
    }
}