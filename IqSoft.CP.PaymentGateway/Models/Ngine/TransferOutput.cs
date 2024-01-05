using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class TransferOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; } = "OK";

        [JsonProperty(PropertyName = "TransactionID")]
        public long TransactionID { get; set; }

        [JsonProperty(PropertyName = "Error")]
        public string Error { get; set; }
    }
}