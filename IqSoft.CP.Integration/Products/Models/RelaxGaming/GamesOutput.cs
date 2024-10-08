using IqSoft.CP.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.RelaxGaming
{
    public class GamesOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "games")]
        public List<GameItem> Games { get; set; }
    }

    public class GameItem
    {
        [JsonProperty(PropertyName = "gameid")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "studio")]
        public string Studio { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "channels")]
        public List<string> Channels { get; set; }

        [JsonProperty(PropertyName = "freespins")]
        public FreespinModel Freespins { get; set; }

        [JsonProperty(PropertyName = "legalbetsizes")]
        public List<int> LegalBetSizes { get; set; }
        public string Currency { get; set; } = Constants.Currencies.Euro;


    }

    public class FreespinModel
    {
        [JsonProperty(PropertyName = "channels")]
        public List<string> Channels { get; set; }
    }
}
