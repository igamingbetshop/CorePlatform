using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.FugaPay
{
    public class PaymentOutput: OutputBase
    {
        [JsonProperty(PropertyName = "response")]
        public ResponseModel ResponseUrl { get; set; }
    }

    public class ResponseModel
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "trnRequestId")]       
        public string RemitRequestId { get; set; }

        [JsonProperty(PropertyName = "RemitRequestId")]
        private string RequestId { set { RemitRequestId = value; } }
    }
}
