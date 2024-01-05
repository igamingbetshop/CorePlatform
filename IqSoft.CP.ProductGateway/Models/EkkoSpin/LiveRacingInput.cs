using Newtonsoft.Json;
using System;

namespace IqSoft.CP.ProductGateway.Models.EkkoSpin
{
    public class LiveRacingInput : BasicInput
    {
        [JsonProperty(PropertyName = "id_feed_bet")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "bet")]
        public decimal Bet { get; set; }

        [JsonProperty(PropertyName = "count")]
        public int CountOfBets { get; set; }

        [JsonProperty(PropertyName = "betting-pool")]
        public string BettingPoolType { get; set; }

        [JsonProperty(PropertyName = "betting-type")]
        public string BettingType { get; set; }

        [JsonProperty(PropertyName = "id_feed_sport")]
        public int SportId { get; set; }

        [JsonProperty(PropertyName = "sport")]
        public string SportTitle { get; set; }

        [JsonProperty(PropertyName = "id_feed_location")]
        public int LocationId { get; set; }

        [JsonProperty(PropertyName = "location")]
        public string LocationTitle { get; set; }

        [JsonProperty(PropertyName = "iso3")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "id_feed_league")]
        public int LeagueId { get; set; }

        [JsonProperty(PropertyName = "league")]
        public string LeagueTitle { get; set; }

        [JsonProperty(PropertyName = "racenumber")]
        public int RaceNumber { get; set; }

        [JsonProperty(PropertyName = "startdate")]
        public DateTime RaceStartDate { get; set; }

        [JsonProperty(PropertyName = "I")]
        public string FirstPlace { get; set; }

        [JsonProperty(PropertyName = "II")]
        public string SecondPlace { get; set; }

        [JsonProperty(PropertyName = "III")]
        public string ThirdPlace { get; set; }

        [JsonProperty(PropertyName = "K")]
        public string AnyPlace { get; set; }

        [JsonProperty(PropertyName = "payout")]
        public decimal Payout { get; set; }

        [JsonProperty(PropertyName = "refund")]
        public decimal Refund { get; set; }
    }
}