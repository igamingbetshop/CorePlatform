using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Evolution
{
    public class OutputBase
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string Uuid { get; set; }
    }
}