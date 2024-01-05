using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class GetGameDetailOutput : BaseResponse
    {
        [JsonProperty(PropertyName = "match_id")]
        public string MatchId { get; set; }

        [JsonProperty(PropertyName = "league_id")]
        public string LeagueId { get; set; }

        [JsonProperty(PropertyName = "home_id")]
        public string HomeId { get; set; }

        [JsonProperty(PropertyName = "away_id")]
        public string AwayId { get; set; }

        [JsonProperty(PropertyName = "ht_home_score")]
        public string HomeScore { get; set; }

        [JsonProperty(PropertyName = "ht_away_score")]
        public string AwayScore { get; set; }

        [JsonProperty(PropertyName = "game_status")]
        public string GameStatus { get; set; }

        [JsonProperty(PropertyName = "sport_type")]
        public int SportType { get; set; }

        [JsonProperty(PropertyName = "Is_neutral")]
        public int IsNeutral { get; set; }
    }
}