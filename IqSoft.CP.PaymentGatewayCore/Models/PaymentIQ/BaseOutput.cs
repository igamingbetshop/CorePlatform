using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.PaymentIQ
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }
        [JsonProperty(PropertyName = "errCode")]
        public int? ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errMsg")]
        public string ErrorMessage { get; set; }
    }
}