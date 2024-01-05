using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.SkyWind
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "error_msg")]
        public string ErrorMsg { get; set; }
    }
}