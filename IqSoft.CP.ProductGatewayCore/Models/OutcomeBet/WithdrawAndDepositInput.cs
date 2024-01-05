using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.OutcomeBet
{
    public class WithdrawAndDepositInput
    {
        [JsonProperty(PropertyName = "callerId")]
        public int CallerId { get; set; }

        [JsonProperty(PropertyName = "playerName")]
        public string PlayerName { get; set; }

        [JsonProperty(PropertyName = "withdraw")]
        public decimal Withdraw { get; set; }

        [JsonProperty(PropertyName = "deposit")]
        public decimal Deposit { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "transactionRef")]
        public string TransactionRef { get; set; }

        [JsonProperty(PropertyName = "gameRoundRef")]
        public string GameRoundRef { get; set; }

        [JsonProperty(PropertyName = "gameId")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "reason")]
        public string Reason { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "sessionAlternativeId")]
        public string SessionAlternativeId { get; set; }
    }

    public class Spin
    {
        [JsonProperty(PropertyName = "betType")]
        public string BetType { get; set; }

        [JsonProperty(PropertyName = "winType")]
        public string WinType { get; set; }
    }
}