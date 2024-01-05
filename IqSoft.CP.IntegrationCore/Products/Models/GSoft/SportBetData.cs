using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class SportBetData : BetDataBase
    {
        [JsonProperty(PropertyName = "LeagueId")]
        public int LeagueId { get; set; }

        [JsonProperty(PropertyName = "LeagueName")]
        public string LeagueName { get; set; }

        [JsonProperty(PropertyName = "AwayId")]
        public long AwayTeamId { get; set; }

        [JsonProperty(PropertyName = "AwayIDName")]
        public string AwayTeamName { get; set; }

        [JsonProperty(PropertyName = "HomeId")]
        public long HomeTeamId { get; set; }

        [JsonProperty(PropertyName = "HomeIDName")]
        public string HomeTeamName { get; set; }

        [JsonProperty(PropertyName = "MatchDateTime")]
        public DateTime MatchDate { get; set; }

        [JsonProperty(PropertyName = "ParlayRefNo")]
        public long ParlayRefNumber { get; set; }

        [JsonProperty(PropertyName = "HDP")]
        public decimal Handicap { get; set; }

        [JsonProperty(PropertyName = "AwayHDP")]
        public decimal AwayHandicap { get; set; }

        [JsonProperty(PropertyName = "HomeHDP")]
        public decimal HomeHandicap { get; set; }

        [JsonProperty(PropertyName = "AwayScore")]
        public decimal AwayTeamScore { get; set; }

        [JsonProperty(PropertyName = "HomeScore")]
        public decimal HomeTeamScore { get; set; }

        [JsonProperty(PropertyName = "IsLive")]
        public string IsLive { get; set; }

        [JsonProperty(PropertyName = "IsLucky")]
        public string IsLucky { get; set; }

        [JsonProperty(PropertyName = "parlay_type")]
        public string ParlayType { get; set; }

        [JsonProperty(PropertyName = "combo_type")]
        public string ComboType { get; set; }

        [JsonProperty(PropertyName = "Bet_tag")]
        public string BetTag { get; set; }

        [JsonProperty(PropertyName = "LastBallNo")]
        public string LastBallNumber { get; set; }

        [JsonProperty(PropertyName = "ParlayData")]
        public List<ParlayData> ParleyInfo { get; set; }
    }
}