using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class BetDataBase
    {
        [JsonProperty(PropertyName = "TransId")]
        public long TransactionExternalId { get; set; }

        [JsonProperty(PropertyName = "PlayerName")]
        public string PlayerName { get; set; }

        [JsonProperty(PropertyName = "TransactionTime")]
        public DateTime TransactionTime { get; set; }

        [JsonProperty(PropertyName = "MatchId")]
        public long MatchId { get; set; }

        [JsonProperty(PropertyName = "SportType")]
        public string SportType { get; set; }

        [JsonProperty(PropertyName = "BetType")]
        public int BetType { get; set; }

        [JsonProperty(PropertyName = "BetTeam")]
        public string BetTeam { get; set; }

        [JsonProperty(PropertyName = "Odds")]
        public decimal Odds { get; set; }

        [JsonProperty(PropertyName = "Stake")]
        public int BetAmount { get; set; }

        [JsonProperty(PropertyName = "WinLoseAmount")]
        public decimal WinLoseAmount { get; set; }

        [JsonProperty(PropertyName = "WinLostDateTime")]
        public DateTime CalculationDate { get; set; }

        [JsonProperty(PropertyName = "OddsType")]
        public int OddsType { get; set; }

        [JsonProperty(PropertyName = "TicketStatus")]
        public string TicketStatus { get; set; }

        [JsonProperty(PropertyName = "Currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "VersionKey")]
        public long VersionKey { get; set; }
    }
}