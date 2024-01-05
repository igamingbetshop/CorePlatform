using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Mifinity
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "traceId")]
        public string TraceId { get; set; }

        [JsonProperty(PropertyName = "money")]
        public Modey MoneyDetails { get; set; }

        [JsonProperty(PropertyName = "clientReference")]
        public string ClientReference { get; set; }

        [JsonProperty(PropertyName = "validationKey")]
        public string ValidationKey { get; set; }

        [JsonProperty(PropertyName = "transactionReference")]
        public string TransactionReference { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }
    }

    public class Modey
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }
}