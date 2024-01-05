using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.OASIS
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "result_key")]
        public int ResultKey { get; set; }
    }
}
