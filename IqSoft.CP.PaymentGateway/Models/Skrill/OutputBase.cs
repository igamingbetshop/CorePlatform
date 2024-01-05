using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Skrill
{
    public class OutputBase
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }
    }
}