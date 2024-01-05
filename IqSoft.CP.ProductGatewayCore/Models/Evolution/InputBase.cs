using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Evolution
{
    public class InputBase
    {
        [JsonProperty(PropertyName = "userId")]
        public int ClientId { get; set; }

        [JsonProperty(PropertyName = "sid")]
        public string Sid { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string Uuid { get; set; }
    }
}