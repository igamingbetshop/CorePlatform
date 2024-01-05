using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class SettingData
    {
        [JsonProperty(PropertyName = "sport_type")]
        public string SportType { get; set; }

        [JsonProperty(PropertyName = "min_bet")]
        public decimal MinBet { get; set; }

        [JsonProperty(PropertyName = "max_bet")]
        public decimal MaxBet { get; set; }

        [JsonProperty(PropertyName = "max_bet_per_match")]
        public decimal MaxBetPerMatch { get; set; }

        [JsonProperty(PropertyName = "max_bet_per_ball")]
        public decimal MaxBetPerBall { get; set; }
    }
}