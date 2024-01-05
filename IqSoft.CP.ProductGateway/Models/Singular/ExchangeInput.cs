using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    public class ExchangeInput : BaseInput
    {
        [JsonProperty(PropertyName = "sourceCurrencyId")]
        public int SourceCurrencyId { get; set; }

        [JsonProperty(PropertyName = "destinationCurrencyId")]
        public int DestinationCurrencyId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "isReverse")]
        public bool IsReverse { get; set; }
    }
}