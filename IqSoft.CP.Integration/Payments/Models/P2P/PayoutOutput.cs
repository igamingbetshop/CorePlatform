using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.P2P
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "withdraw_id")]
        public string WithdrawId { get; set; }

        [JsonProperty(PropertyName = "payment_system_id")]
        public string PaymentSystemId { get; set; }

        [JsonProperty(PropertyName = "received_withdraw_id")]
        public string ReceivedWithdrawId { get; set; }
    }
}
