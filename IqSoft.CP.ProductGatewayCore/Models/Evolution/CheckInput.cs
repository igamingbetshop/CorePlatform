using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Evolution
{
    public class CheckInput : InputBase
    {
        [JsonProperty(PropertyName = "channel")]
        public Channel Channel { get; set; }
    }

    public class Channel
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}