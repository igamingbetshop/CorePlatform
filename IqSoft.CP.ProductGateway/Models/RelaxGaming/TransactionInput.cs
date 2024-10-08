using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.RelaxGaming
{
    public class TransactionInput : BaseInput
    {

        [JsonProperty(PropertyName = "gamesessionid")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "txid")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "txtype")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "ended")]
        public bool CloseRound { get; set; }
    }
}