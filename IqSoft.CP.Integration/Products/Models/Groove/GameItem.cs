using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.Groove
{
    public class GameItem
    {
        [JsonProperty(PropertyName = "gameId")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "gameName")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "gameCategory")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "platforms")]
        public List< string> Platforms { get; set; }

        [JsonProperty(PropertyName = "defaultImg")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "subVendorName")]
        public string SubProvider { get; set; }

        [JsonProperty(PropertyName = "supportFrb")]
        public string SupportFreeBet { get; set; }

        [JsonProperty(PropertyName = "defaultRtp")]
        public string DefaultRtp { get; set; }
    }
}