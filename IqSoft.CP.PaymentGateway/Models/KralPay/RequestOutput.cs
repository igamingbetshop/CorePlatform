using Newtonsoft.Json;
namespace IqSoft.CP.PaymentGateway.Models.KralPay
{
    public class RequestOutput
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; } = "200";

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; } = "Success";
    }
}