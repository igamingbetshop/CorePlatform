using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class LoginOutput : BaseResponse
    {
        [JsonProperty(PropertyName = "Data")]
        public string Data { get; set; }
    }
}