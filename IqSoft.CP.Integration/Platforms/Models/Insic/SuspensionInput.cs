using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class SuspensionInput : RequestBase
    {
        [JsonProperty(PropertyName = "suspensionReason")]
        public string SuspensionReason { get; set; }

        [JsonProperty(PropertyName = "suspensionEndsAt")]
        public string SuspensionEndsAt { get; set; }
    }
}
