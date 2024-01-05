using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Pix
{
    public class PaymentWithdrawInput
    {
        [JsonProperty(PropertyName = "uuid")]
        public string UuId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

        [JsonProperty(PropertyName = "scheduling_date")]
        public string SchedulingDate { get; set; }

        [JsonProperty(PropertyName = "confirmed_at")]
        public string ConfirmedAt { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}