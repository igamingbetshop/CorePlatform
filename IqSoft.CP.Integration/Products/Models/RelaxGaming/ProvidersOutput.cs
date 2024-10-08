using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.RelaxGaming
{
    public class ProviderItem
    {
        [JsonProperty(PropertyName = "platformid")]
        public string PlatformId { get; set; }

        [JsonProperty(PropertyName = "provider")]
        public string Provider { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}