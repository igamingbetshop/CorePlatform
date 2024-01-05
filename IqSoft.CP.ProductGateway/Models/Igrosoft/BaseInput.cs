using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Igrosoft
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "server")]
        public string Server { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "gameId")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "gameRound")]
        public string GameRound { get; set; }

        [JsonProperty(PropertyName = "time")]
        public string Time { get; set; }

        [JsonProperty(PropertyName = "deposit")]
        public decimal? Deposit { get; set; }

        [JsonProperty(PropertyName = "withdraw")]
        public decimal? Withdraw { get; set; }

        [JsonProperty(PropertyName = "bonus")]
        public decimal? Bonus { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "denom")]
        public decimal? Denom { get; set; }

        [JsonProperty(PropertyName = "previousTrxId")]
        public string PreviousTrxId { get; set; }

        [JsonProperty(PropertyName = "roundFinished")]
        public string roundFinished { get; set; }

        [JsonProperty(PropertyName = "freeSpinsLeft")]
        public string freeSpinsLeft { get; set; }
    }
}