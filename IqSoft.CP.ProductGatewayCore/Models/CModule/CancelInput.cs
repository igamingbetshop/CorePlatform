using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.CModule
{
    public class CancelInput
    {
        [JsonProperty(PropertyName = "cmd")]
        public string Command { get; set; }

       [JsonProperty(PropertyName = "body")]
        public BaseInput Body { get; set; }
    }
}