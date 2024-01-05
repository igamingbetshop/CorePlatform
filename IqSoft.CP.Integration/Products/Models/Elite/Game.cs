using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.Elite
{
    public class Game
    {
        [JsonProperty(PropertyName = "gameName")]
        public string GameName { get; set; }

        [JsonProperty(PropertyName = "gameID")]
        public string GameID { get; set; }

        [JsonProperty(PropertyName = "gameDescription")]
        public string GameDescription { get; set; }

        [JsonProperty(PropertyName = "gameVendor")]
        public string GameVendor { get; set; }

        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "subCategory")]
        public string SubCategory { get; set; }

        [JsonProperty(PropertyName = "gameLaunchUrl")]
        public string GameLaunchUrl { get; set; }

        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "skinCode")]
        public string SkinCode { get; set; }

        [JsonProperty(PropertyName = "gameInfo")]
        public GameInfo GameInfo { get; set; }

        [JsonProperty(PropertyName = "gameNames")]
        public GameNames GameNames { get; set; }

        [JsonProperty(PropertyName = "supportsBonusPlay")]
        public bool SupportsBonusPlay { get; set; }

        [JsonProperty(PropertyName = "supportsCryptoCurrency")]
        public bool SupportsCryptoCurrency { get; set; }

        [JsonProperty(PropertyName = "allowFreeRounds")]
        public bool AllowFreeRounds { get; set; }
    }
    public class GameInfo
    {

        [JsonProperty(PropertyName = "rtp")]
        public string Rtp { get; set; }
    }

    public class GameNames
    { 
    
    }
}
