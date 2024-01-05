using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Evolution
{
    public class FinOperationInput : InputBase
    {
        [JsonProperty(PropertyName = "currency")]
        public string CurrencyId { get; set; }

        [JsonProperty(PropertyName = "game")]
        public Game Game { get; set; }

        [JsonProperty(PropertyName = "transaction")]
        public Transaction Transaction { get; set; }
    }
}
