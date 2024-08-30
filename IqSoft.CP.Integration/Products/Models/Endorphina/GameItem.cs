using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.Endorphina
{
    public class GameItem
    {
        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "bonuses")]
        public List<string> Bonuses { get; set; }

        [JsonProperty(PropertyName = "bets")]
        public List<int> Bets { get; set; }
    }
}
