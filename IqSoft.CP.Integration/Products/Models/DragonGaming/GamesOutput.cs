using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.DragonGaming
{
    public class GamesOutput
    {
        [JsonProperty(PropertyName = "result")]
        public Categories Categories { get; set; }
    }

    public class Categories
    {
        [JsonProperty(PropertyName = "slots")]
        public List<object> Slots { get; set; }

        [JsonProperty(PropertyName = "table_games")]
        public List<object> TableGames { get; set; }

        [JsonProperty(PropertyName = "scratch_cards")]
        public List<object> ScratchCards { get; set; }
    }
    public class GameProperty
    {
        [JsonProperty(PropertyName = "game_id")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "game_name")]
        public string GameName { get; set; }

        [JsonProperty(PropertyName = "game_title")]
        public string GameTitle { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "supplier")]
        public string Supplier { get; set; }

        [JsonProperty(PropertyName = "logos")]
        public List<Logo> Logos { get; set; }
    }

    public class Logo
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
