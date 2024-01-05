using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SoftSwiss
{
    public class BaseError
    {
        [JsonProperty(PropertyName = "code")]
        public int? Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}