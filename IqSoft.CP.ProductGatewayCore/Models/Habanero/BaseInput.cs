using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Habanero
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "type")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "dtsent")]
        public string DateSent { get; set; }
    }

    public class Game
    {
        [JsonProperty(PropertyName = "brandgameid")]
        public string BrandId { get; set; }

        [JsonProperty(PropertyName = "keyname")]
        public string KeyName { get; set; }
    }

  public class GameSession
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "brandgameid")]
        public string BrandGameId { get; set; }

      [JsonProperty(PropertyName = "gamesessionid")]
        public string GameSessionId { get; set; }
    }
}