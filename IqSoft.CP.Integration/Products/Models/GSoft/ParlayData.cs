using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class ParlayData
    {
        [JsonProperty(PropertyName = "ParlayRefNo")]
        public long ParlayRefNumber { get; set; }

        [JsonProperty(PropertyName = "Parlay_LeagueID")]
        public int ParlayLeagueId { get; set; }

        [JsonProperty(PropertyName = "LeagueName")]
        public string LeagueName { get; set; }

        [JsonProperty(PropertyName = "Parlay_MatchID")]
        public long ParlayMatchId { get; set; }

        [JsonProperty(PropertyName = "Parlay_AwayID")]
        public long ParlayAwayId { get; set; }

        [JsonProperty(PropertyName = "Parlay_AwayIDName")]
        public string ParlayAwayIdName { get; set; }

        [JsonProperty(PropertyName = "Parlay_HomeID")]
        public long ParlayHomeTeamId { get; set; }

        [JsonProperty(PropertyName = "Parlay_HomeIDName")]
        public string ParlayHomeTeamName { get; set; }

        [JsonProperty(PropertyName = "Parlay_MatchDateTime")]
        public DateTime ParlayMatchDate { get; set; }

        [JsonProperty(PropertyName = "Parlay_Odds")]
        public decimal ParlayOdds { get; set; }

        [JsonProperty(PropertyName = "Parlay_BetType")]
        public int ParlayBetType { get; set; }

        [JsonProperty(PropertyName = "Parlay_BetTeam")]
        public string ParlayBetTeam { get; set; }

        [JsonProperty(PropertyName = "Parlay_SportType")]
        public int ParlaySportType { get; set; }

        [JsonProperty(PropertyName = "Parlay_HomeHDP")]
        public decimal ParlayHomeHandicap { get; set; }

        [JsonProperty(PropertyName = "Parlay_AwayHDP")]
        public decimal ParlayAwayHandicap { get; set; }

        [JsonProperty(PropertyName = "Parlay_HDP")]
        public decimal ParlayHandicap { get; set; }

        [JsonProperty(PropertyName = "Parlay_isLive")]
        public string ParlayIsLive { get; set; }

        [JsonProperty(PropertyName = "Parlay_HomeScore")]
        public decimal ParlayHomeScore { get; set; }

        [JsonProperty(PropertyName = "Parlay_AwayScore")]
        public decimal ParlayAwayScore { get; set; }

        [JsonProperty(PropertyName = "Parlay_WinLost")]
        public string ParlayWinLost { get; set; }

        [JsonProperty(PropertyName = "Parlay_WinLostDateTime")]
        public DateTime ParlayCalculationDate { get; set; }

        [JsonProperty(PropertyName = "Parlay_IsLucky")]
        public string ParlayIsLucky { get; set; }

        [JsonProperty(PropertyName = "Parlay_Bet_tag")]
        public string ParlayBetTag { get; set; }
    }
}