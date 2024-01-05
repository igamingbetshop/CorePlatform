using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Pix
{
    public class PaymentDepositInput
    {
        [JsonProperty(PropertyName = "conciliation_id")]
        public string ConciliationId { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty(PropertyName = "buyer_name")]
        public string BuyerName { get; set; }

        [JsonProperty(PropertyName = "description")]
        public object Description { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

    }
}