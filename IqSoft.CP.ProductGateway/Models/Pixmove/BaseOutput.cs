using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Pixmove
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; } = "success";

        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        [JsonProperty(PropertyName = "error")]
        public object Error { get; set; }
    }
}