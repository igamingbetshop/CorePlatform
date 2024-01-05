using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.SoftSwiss
{
    public class BaseOutput : BaseError
    {

        [JsonProperty(PropertyName = "balance")]
        public int? Balance { get; set; }

        [JsonProperty(PropertyName = "game_id")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "game")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "transactions")]
        public List<Transaction> Transactions { get; set; }
    }

    public class Transaction
    {
        [JsonProperty(PropertyName = "action_id")]
        public string ActionId { get; set; }

        [JsonProperty(PropertyName = "tx_id")]
        public string TxId { get; set; }
    }
}