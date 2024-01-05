using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.PaySec
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "header")]
        public OutputHeader outputHeader { get; set; }

        [JsonProperty(PropertyName = "body")]
        public OutputBody outputBody { get; set; }
    }

    public class OutputHeader
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "statusMessage")]
        public string Message { get; set; }
    }

    public class OutputBody
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }
}
