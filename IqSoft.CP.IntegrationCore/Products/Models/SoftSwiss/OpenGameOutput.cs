using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.SoftSwiss
{
    public class OpenGameOutput
    {
        [JsonProperty(PropertyName = "launch_options")]
        public LaunchOptions LaunchParameters { get; set; }
    }

    public class LaunchOptions
    {
        [JsonProperty(PropertyName = "game_url")]
        public string GameUrl { get; set; }

        [JsonProperty(PropertyName = "strategy")]
        public string Strategy { get; set; }
    }
}