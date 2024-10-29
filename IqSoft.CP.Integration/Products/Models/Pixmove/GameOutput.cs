using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.Pixmove
{
    public class GameOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }
    }

    public class DataModel
    {
        [JsonProperty(PropertyName = "games")]
        public List<GameItem> Games { get; set; }
    }

    public class GameItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "class")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }
    }
}
