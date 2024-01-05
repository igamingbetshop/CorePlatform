using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class LimitInput : RequestBase
    {
        [JsonProperty(PropertyName = "limitScope")]
        public string LimitScope { get; set; }

        [JsonProperty(PropertyName = "timeUnit")]
        public string TimeUnit { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal? Amount { get; set; }
    }
}
