using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.TotalProcessing
{
    public class CheckPaymentStateOutput
    {
        [JsonProperty("result")]
        public PaymentResult Result { get; set; }
    }

    public class PaymentResult
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}