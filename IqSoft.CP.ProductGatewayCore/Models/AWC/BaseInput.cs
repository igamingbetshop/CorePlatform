using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.AWC
{
    public class BaseInput
    {
        public string key { get; set; }
        public string extension1 { get; set; }

        public string message { get; set; }
    }

    public class MessageModel
    {
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "txns")]
        public List<Transaction> Transactions { get; set; }
    }

    public class Transaction
    {
        [JsonProperty(PropertyName = "platformTxId")]
        public string PlatformTxId { get; set; }
        
        [JsonProperty(PropertyName = "refPlatformTxId")]
        public string RefPlatformTxId { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "platform")]
        public string Platform { get; set; }

        [JsonProperty(PropertyName = "gameType")]
        public string GameType { get; set; }

        [JsonProperty(PropertyName = "gameCode")]
        public string GameCode { get; set; }

        [JsonProperty(PropertyName = "gameName")]
        public string GameName { get; set; }

        [JsonProperty(PropertyName = "betType")]
        public string BetType { get; set; }

        [JsonProperty(PropertyName = "betAmount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "adjustAmount")]
        public decimal AdjustAmount { get; set; }

        [JsonProperty(PropertyName = "winAmount")]
        public decimal? WinAmount { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal BonusAmount { get; set; }

        [JsonProperty(PropertyName = "betTime")]
        public string BetTime { get; set; }

        [JsonProperty(PropertyName = "roundId")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "gameInfo")]
        public GameInfo GameInfoItem { get; set; }

        [JsonProperty(PropertyName = "promotionTxId")]
        public string PromotionTxId { get; set; }

        [JsonProperty(PropertyName = "promotionId")]
        public string PromotionId { get; set; }

        [JsonProperty(PropertyName = "promotionTypeId")]
        public string PromotionTypeId { get; set; }

        [JsonProperty(PropertyName = "txTime")]
        public string txTime { get; set; }

        [JsonProperty(PropertyName = "turnover")]
        public decimal Turnover { get; set; }

        [JsonProperty(PropertyName = "updateTime")]
        public string UpdateTime { get; set; }

        [JsonProperty(PropertyName = "settleType")]
        public string SettleType { get; set; }

        [JsonProperty(PropertyName = "tip")]
        public decimal TipAmount { get; set; }

    }

    public class GameInfo
    {
        //[JsonProperty(PropertyName = "odds")]
        //public decimal Odds { get; set; }

        [JsonProperty(PropertyName = "winLoss")]
        public decimal WinLoss { get; set; }
    }
}
    