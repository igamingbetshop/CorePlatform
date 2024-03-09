using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.AleaPartners
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; } = 200;

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "result")]
        public object Result { get; set; }
    }
}