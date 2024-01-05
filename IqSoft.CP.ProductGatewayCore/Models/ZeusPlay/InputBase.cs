using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ZeusPlay
{
    public class InputBase
    {
        [JsonProperty(PropertyName = "func")]
        public string Func { get; set; }

        [JsonProperty(PropertyName = "game_session_id")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "datasig")]
        public string DataSignature { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "transaction_id_ns")]
        public string TransactionIdNs { get; set; }

        [JsonProperty(PropertyName = "bet_id_ns")]
        public string BetTransactionId { get; set; }

        [JsonProperty(PropertyName = "random_number")]
        public string RandomNumber { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "transfer_in")]
        public string TransferIn { get; set; }

        [JsonProperty(PropertyName = "transfer_out")]
        public string TransferOut { get; set; }

        [JsonProperty(PropertyName = "bets_sum")]
        public string BetsSum { get; set; }

        [JsonProperty(PropertyName = "bets_count")]
        public int BetsCount { get; set; }

        [JsonProperty(PropertyName = "wins_sum")]
        public string WinsSum { get; set; }

        [JsonProperty(PropertyName = "wins_count")]
        public int WinsCount { get; set; }
    }
}