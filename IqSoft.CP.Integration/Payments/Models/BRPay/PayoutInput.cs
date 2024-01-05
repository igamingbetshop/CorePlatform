using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.BRPay
{
    public class PayoutInput
    {
        [JsonProperty(PropertyName = "sp_outlet_id")]
        public long OutletId { get; set; }

        [JsonProperty(PropertyName = "sp_order_id")]
        public long OrderId { get; set; }

        [JsonProperty(PropertyName = "sp_amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "sp_destination_card")]
        public string DestinationCard { get; set; }

        [JsonProperty(PropertyName = "sp_salt")]
        public string Salt { get; set; }

        [JsonProperty(PropertyName = "sp_sig")]
        public string Signature { get; set; }
    }
}
