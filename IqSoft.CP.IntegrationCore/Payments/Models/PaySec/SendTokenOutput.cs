using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.PaySec
{
    public class SendTokenOutput
    {
        [JsonProperty(PropertyName = "qrCode")]
        public string QrCode { get; set; }

        [JsonProperty(PropertyName = "transactionReference")]
        public string ExternalTransactionId { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "statusMessage")]
        public string StatusMessage { get; set; }
    }

    public class SendPayoutTokenOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string StatusMessage { get; set; }
    }
}