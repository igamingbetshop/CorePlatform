using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.IPTGaming
{
    public class LaunchOutput
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
