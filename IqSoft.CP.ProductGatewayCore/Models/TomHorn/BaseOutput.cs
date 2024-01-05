using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TomHorn
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "Code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "Message")]
        public string Message { get; set; }
    }
}