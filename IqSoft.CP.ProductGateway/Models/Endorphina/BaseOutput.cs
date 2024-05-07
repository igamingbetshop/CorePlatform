using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Endorphina
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "player")]
        public string Player { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "game")]
        public string Game { get; set; }
    }
}