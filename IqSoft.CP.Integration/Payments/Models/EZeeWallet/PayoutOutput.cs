using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.EZeeWallet
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "unique_id")]
        public string ExternalTransactionId{ get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "TransactionId")]
        public long TransactionId { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
