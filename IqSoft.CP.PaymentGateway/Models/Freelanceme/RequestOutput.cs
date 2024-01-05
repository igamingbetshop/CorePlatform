using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Freelanceme
{
    public class RequestOutput
    {
        [JsonProperty(PropertyName = "auth")]
        public ErrorType Error { get; set; }
    }
    public class ErrorType
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

    }
}