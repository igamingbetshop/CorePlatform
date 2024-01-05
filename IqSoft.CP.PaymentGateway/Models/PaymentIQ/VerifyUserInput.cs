using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.PaymentIQ
{
    public class VerifyUserInput
    {
        [JsonProperty(PropertyName = "userId")]
        public int ClientId { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }
    }
}