using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.XcoinsPay
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }
    }
}