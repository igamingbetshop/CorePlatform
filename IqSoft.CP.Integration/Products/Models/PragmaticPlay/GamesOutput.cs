using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.PragmaticPlay
{

    public class GamesOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "gameList")]
        public List<GameItem> GamesList { get; set; }
    }

    public class GameItem
    {
        [JsonProperty(PropertyName = "gameID")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "gameName")]
        public string GameName { get; set; }

        [JsonProperty(PropertyName = "gameTypeID")]
        public string GameTypeID { get; set; }

        [JsonProperty(PropertyName = "typeDescription")]
        public string TypeDescription { get; set; }

        [JsonProperty(PropertyName = "technology")]
        public string technology { get; set; }

        [JsonProperty(PropertyName = "platform")]
        public string Platform { get; set; }

        [JsonProperty(PropertyName = "demoGameAvailable")]
        public bool DemoGameAvailable { get; set; }

        [JsonProperty(PropertyName = "frbAvailable")]
        public bool? FreeRoundAvailable { get; set; }

        [JsonProperty(PropertyName = "betScaleList")]
        public Dictionary<string, List<decimal>> BetScaleList { get; set; }
    }

    public class GameBetValueOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "gameList")]
        public List<BetScale> GamesList { get; set; }
    }

    public class BetScale
    {
        [JsonProperty(PropertyName = "gameID")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "betScaleList")]
        public List<BetScaleItem> BetScaleList { get; set; }
    }

    public class BetScaleItem
    {
        [JsonProperty(PropertyName = "betPerLineScales")]
        public List<decimal> BetPerLineScales { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "totalBetScales")]
        public List<decimal> TotalBetScales { get; set; }
    }
}