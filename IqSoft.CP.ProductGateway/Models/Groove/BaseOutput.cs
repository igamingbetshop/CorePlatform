using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Groove
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "accountid")]
        public int ClientId { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "gamesessionid")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "real_balance")]
        public decimal RealBalance { get; set; }

        [JsonProperty(PropertyName = "bonus_balance")]
        public decimal BonusBalance { get; set; }

        [JsonProperty(PropertyName = "game_mode")]
        public int? GameMode { get; set; }

        [JsonProperty(PropertyName = "apiversion")]
        public string ApiVersion { get; set; }

        [JsonProperty(PropertyName = "order")]
        public string Order { get; set; }

        [JsonProperty(PropertyName = "accounttransactionid")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal? Balance { get; set; }

        [JsonProperty(PropertyName = "bonusmoneybet")]
        public decimal? BonusMoneyBet { get; set; }

        [JsonProperty(PropertyName = "realmoneybet")]
        public decimal? RealMoneyBet { get; set; }

        [JsonProperty(PropertyName = "walletTx")]
        public string WinTransactionId { get; set; }

        [JsonProperty(PropertyName = "bonusWin")]
        public decimal? BonusWin { get; set; }

        [JsonProperty(PropertyName = "realMoneyWin")]
        public decimal? RealMoneyWin { get; set; }

    }
}