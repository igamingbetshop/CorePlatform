using Newtonsoft.Json;

namespace IqSoft.CP.Common.Models.WebSiteModels.Bets
{
    public class TurnoverModel
    {
        [JsonProperty(PropertyName = "SumStake")]
        public decimal BetAmount { get; set; }

        [JsonProperty(PropertyName = "SumWinnings")]
        public decimal WinAmount { get; set; }
    }
}
