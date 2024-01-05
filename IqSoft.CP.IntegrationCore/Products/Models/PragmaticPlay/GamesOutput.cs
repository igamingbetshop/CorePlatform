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
    }
}
