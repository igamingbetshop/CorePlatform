using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.Webflow
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "_id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
