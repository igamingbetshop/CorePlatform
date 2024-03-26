using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.KralPay
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}