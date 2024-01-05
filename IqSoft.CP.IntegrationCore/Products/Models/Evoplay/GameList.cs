using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.Evoplay
{
    public class GameList
    {
        public string Status { get; set; }

        public Dictionary<int,GameItem> Data { get; set; }
    }

    public class GameItem
    {
        public string Name { get; set; }

        [JsonProperty(PropertyName = "game_sub_type")]
        public string Type { get; set; }

        public int Mobile { get; set; }

        public int Desktop { get; set; }

        [JsonProperty(PropertyName = "extra_bonuses_types")]
        public List<string> BonusTypes { get; set; }
    }
}
