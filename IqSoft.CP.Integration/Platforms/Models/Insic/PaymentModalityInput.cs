using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.Insic
{
    public class PaymentModalityInput : RequestBase
    {
        [JsonProperty(PropertyName = "paymentModalityId")]
        public string PaymentModalityId { get; set; }

        [JsonProperty(PropertyName = "methodName")]
        public string MethodName { get; set; }

        [JsonProperty(PropertyName = "idData")]
        public string IdData { get; set; }
    }
}
