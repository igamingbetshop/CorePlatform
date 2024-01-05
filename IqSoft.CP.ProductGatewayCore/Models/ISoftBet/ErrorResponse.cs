using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ISoftBet
{
    public class ErrorResponse : BaseOutput
    {
        [JsonProperty(PropertyName = "code")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "display")]
        public string Display { get; set; }
    }
}