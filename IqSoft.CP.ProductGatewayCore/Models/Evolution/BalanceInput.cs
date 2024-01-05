using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Evolution
{
    public class BalanceInput : InputBase
    {
        [JsonProperty(PropertyName = "game")]
        public Game Game { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string CurrencyId { get; set; }
    }
}
