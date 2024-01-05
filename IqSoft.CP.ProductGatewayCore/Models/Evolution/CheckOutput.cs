using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Evolution
{
    public class CheckOutput : OutputBase
    {
        [JsonProperty(PropertyName = "sid")]
        public string Sid { get; set; }
    }
}