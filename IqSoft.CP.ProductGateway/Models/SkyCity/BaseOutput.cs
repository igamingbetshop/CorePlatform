using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SkyCity
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "result")]
        public int Result { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "rtn")]
        public string Description { get; set; }
    }
}