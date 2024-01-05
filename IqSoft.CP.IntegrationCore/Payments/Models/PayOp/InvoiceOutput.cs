using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.PayOp
{
    public class InvoiceOutput
    {
        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
