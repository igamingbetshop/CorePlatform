using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.FreeKassa
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }

        [JsonProperty(PropertyName = "desc")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

    }

    public class PaymentData
    {
        [JsonProperty(PropertyName = "payment_id")]
        public string PaymentId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}
