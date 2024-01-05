using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class PaymentInput : RequestBase
    {
        [JsonProperty(PropertyName = "depositId")]
        public string DepositId { get; set; }

        [JsonProperty(PropertyName = "payoutId")]
        public string PayoutId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "automatic")]
        public bool? Automatic { get; set; }
    }
}
