using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.Evolution
{
    public class ReportOutput
    {
        [JsonProperty(PropertyName = "timestamp")]
        public string RequestDate { get; set; }

        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }
    }

    public class DataModel
    {
        [JsonProperty(PropertyName = "id")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "startedAt")]
        public string RoundStartTime { get; set; }

        [JsonProperty(PropertyName = "settledAt")]
        public string SettledTime { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "gameType")]
        public string GameType { get; set; }

        [JsonProperty(PropertyName = "gameSubType")]
        public string GameSubType { get; set; }

        [JsonProperty(PropertyName = "dealer")]
        public DealerModel Dealer { get; set; }

        [JsonProperty(PropertyName = "table")]
        public TableModel Table { get; set; }

        [JsonProperty(PropertyName = "result")]
        public object Result { get; set; }

        [JsonProperty(PropertyName = "participants")]
        public List<ParticipantModel> Participants { get; set; }
    }

    public class TableModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    public class DealerModel
    {
        [JsonProperty(PropertyName = "uid")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    public class ParticipantModel
    {
        [JsonProperty(PropertyName = "playerId")]
        public string PlayerId { get; set; }

        [JsonProperty(PropertyName = "screenName")]
        public string ScreenName { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "casinoSessionId")]
        public string CasinoSessionId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "bets")]
        public List<BetModel> Bets { get; set; }
    }

    public class BetModel
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "stake")]
        public decimal BetAmount { get; set; }

        [JsonProperty(PropertyName = "payout")]
        public decimal WinAmount { get; set; }

        [JsonProperty(PropertyName = "placedOn")]
        public DateTime BetTime { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }
    }
}
