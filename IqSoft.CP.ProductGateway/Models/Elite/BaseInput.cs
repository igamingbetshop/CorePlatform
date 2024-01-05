using Newtonsoft.Json;
using System;

namespace IqSoft.CP.ProductGateway.Models.Elite
{
    public class Transaction
    {
        [JsonProperty(PropertyName = "sessionToken")]
        public string SessionToken { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "gameId")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "bonusBalance")]
        public decimal BonusBalance { get; set; } = 0;

        [JsonProperty(PropertyName = "walletStrategy")]
        public string WalletStrategy { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "bonusConversion")]
        public bool BonusConversion { get; set; }

        [JsonProperty(PropertyName = "freeSpinWin")]
        public bool FreeSpinWin { get; set; }

        [JsonProperty(PropertyName = "jackpotWin")]
        public bool JackpotWin { get; set; }

        [JsonProperty(PropertyName = "gameRoundId")]
        public string GameRoundId { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "debitTransactionId")]
        public string DebitTransactionId { get; set; }

        [JsonProperty(PropertyName = "creditTransactionId")]
        public string CreditTransactionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "debitAmount")]
        public decimal DebitAmount { get; set; }

        [JsonProperty(PropertyName = "creditAmount")]
        public decimal CreditAmount { get; set; }

        [JsonProperty(PropertyName = "transactionTimeStamp")]
        public DateTime TransactionTimeStamp { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "roundCompleted")]
        public bool RoundCompleted { get; set; }

        [JsonProperty(PropertyName = "gameRoundStartedAt")]
        public DateTime GameRoundStartedAt { get; set; }

        [JsonProperty(PropertyName = "promoWinAmount")]
        public decimal PromoWinAmount { get; set; }

        [JsonProperty(PropertyName = "promoWinReference")]
        public string PromoWinReference { get; set; }
    }
    public class BaseInput
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "params")]
        public Transaction TransactionData { get; set; }
    }
}