using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.GoldenRace
{
    public class GameModel
    {
        [JsonProperty(PropertyName = "status")]
        public bool Status { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Messsage { get; set; }

        [JsonProperty(PropertyName = "data")]
        public List<GameItem> Items { get; set; }
    }

    public class GameItem
    {
        [JsonProperty(PropertyName = "gameId")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "gameFriendlyName")]
        public string GameFriendlyName { get; set; }

        [JsonProperty(PropertyName = "gameTitle")]
        public string GameTitle { get; set; }

        [JsonProperty(PropertyName = "provider")]
        public string Provider { get; set; }

        [JsonProperty(PropertyName = "thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "sub_type")]
        public string SubType { get; set; }

        [JsonProperty(PropertyName = "platforms")]
        public List<string> Platforms { get; set; }

        [JsonProperty(PropertyName = "jurisdictions")]
        public List<string> jurisdictions { get; set; }

        [JsonProperty(PropertyName = "language")]
        public List<string> language { get; set; }
    }
}
