using Newtonsoft.Json;

namespace IqSoft.CP.Common.Models
{
    public class ClickatellInput
    {
        [JsonProperty(PropertyName = "apiKey")]
        public string apiKey { get; set; }

        [JsonProperty(PropertyName = "to")]
        public string to { get; set; }

        [JsonProperty(PropertyName = "content")]
        public string content { get; set; }
    }
}
