using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SkyWind
{
    public class CreditInput :DebitInput
    {
        [JsonProperty(PropertyName = "game_status")]
        public string GameStatus { get; set; }
    }
}