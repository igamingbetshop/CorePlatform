using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.EkkoSpin
{
    public class SlotsInput : BasicInput
    {
        [JsonProperty(PropertyName = "id_stat")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public int TransactionType { get; set; }

        [JsonProperty(PropertyName = "bet")]
        public decimal BetAmount { get; set; }

        [JsonProperty(PropertyName = "win")]
        public decimal WinAmount { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal ClientBalance { get; set; }
    }
}