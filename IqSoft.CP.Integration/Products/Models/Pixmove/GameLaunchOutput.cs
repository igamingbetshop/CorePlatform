using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.Pixmove
{
    public class GameLaunchOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "data")]
        public UrlModel Data { get; set; }
    }

    public class UrlModel
    {
        [JsonProperty(PropertyName = "gameUrl")]
        public string GameUrl { get; set; }
    }
}