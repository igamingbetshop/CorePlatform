using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Interkassa
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "resultCode")]
        public int ResultCode { get; set; }

        [JsonProperty(PropertyName = "resultMsg")]
        public string ResultMsg { get; set; }

        [JsonProperty(PropertyName = "resultData")]
        public ResultData ResultData { get; set; }
    }

    public class Parameters
    {
        [JsonProperty("x-request-id")]
        public string Xrequestid { get; set; }

        [JsonProperty("x-using-mode")]
        public string Xusingmode { get; set; }
    }

    public class PaymentForm
    {
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "parameters")]
        public Parameters Parameters { get; set; }
    }

    public class ResultData
    {
        [JsonProperty(PropertyName = "paymentForm")]
        public PaymentForm PaymentForm { get; set; }
    }
}
