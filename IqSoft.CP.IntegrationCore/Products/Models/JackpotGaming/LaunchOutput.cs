using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.JackpotGaming
{
    public class LaunchOutput
    {
        [JsonProperty(PropertyName = "launch")]
        public string LaunchUrl { get; set; }
    }
}
