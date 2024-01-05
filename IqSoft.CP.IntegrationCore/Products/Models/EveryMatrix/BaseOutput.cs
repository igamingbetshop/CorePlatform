using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.EveryMatrix
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }   
}
