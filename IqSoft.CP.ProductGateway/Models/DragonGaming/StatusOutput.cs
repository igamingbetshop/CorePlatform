using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.DragonGaming
{
    public class StatusOutput
    {
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; } = 1;

        [JsonProperty(PropertyName = "error_id")]
        public int? ErrorId { get; set; }

        [JsonProperty(PropertyName = "error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "'error_message")]
        public string ErrorMessage { get; set; }
    }
}