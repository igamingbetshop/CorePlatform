using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ISoftBet
{
    public class WinInput :BetInput
    {
        [JsonProperty(PropertyName = "closeround")]
        public string CloseRound { get; set; }
    }
}