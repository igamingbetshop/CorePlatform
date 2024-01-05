using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    public class BalanceInput : BaseInput
    {
        [JsonProperty(PropertyName = "userId")]
        public long UserId { get; set; }

        [JsonProperty(PropertyName = "currencyId")]
        public string CurrencyId { get; set; }

        [JsonProperty(PropertyName = "isSingle")]
        public bool IsSingle { get; set; }
    }
}