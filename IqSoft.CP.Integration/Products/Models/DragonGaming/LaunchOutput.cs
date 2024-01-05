using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.DragonGaming
{
    public class LaunchResultOutput
    {
        [JsonProperty(PropertyName = "result")]
        public LaunchOutput LaunchResult { get; set; }
    }

    public class LaunchOutput
    {
        [JsonProperty(PropertyName = "launch_url")]
        public string Url { get; set; }
    }
}
