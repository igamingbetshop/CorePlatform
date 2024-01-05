using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.PragmaticPlay
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "error")]
        public int ErrorCode { get; set; }
    }
}