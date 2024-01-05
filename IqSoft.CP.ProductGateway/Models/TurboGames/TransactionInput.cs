using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TurboGames
{
    public class TransactionInput : BalanceInput
    {
        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal? Amount { get; set; }

        [JsonProperty(PropertyName = "type")]
        public int Type { get; set; }
    }
}