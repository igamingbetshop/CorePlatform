using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.PlaynGo
{
    public class GameItem
    {

        [JsonProperty(PropertyName = "gameId")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "gameType")]
        public string GameType { get; set; }

        [JsonProperty(PropertyName = "gid")]
        public string GId { get; set; }
    }
}
