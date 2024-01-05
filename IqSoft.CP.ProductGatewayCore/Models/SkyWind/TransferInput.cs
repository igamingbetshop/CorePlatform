using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SkyWind
{
    public class TransferInput : DebitInput
    {
        [JsonProperty(PropertyName = "game_status")]
        public string GameStatus { get; set; }

        [JsonProperty(PropertyName = "actual_bet_amount")]
        public decimal ActualBetAmount { get; set; }

        [JsonProperty(PropertyName = "actual_win_amount")]
        public decimal ActualWinAmount { get; set; }

        [JsonProperty(PropertyName = "jp_win_amount")]
        public decimal JackpotWinAmount { get; set; }

        [JsonProperty(PropertyName = "left_amount")]
        public decimal LeftAmount { get; set; }
    }
}