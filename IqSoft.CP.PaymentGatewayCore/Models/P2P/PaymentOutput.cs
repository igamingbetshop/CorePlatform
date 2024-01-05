using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.P2P
{
   public  class PaymentOutput
    {
        [JsonProperty(PropertyName = "deposit_id")]
        public string DepositId { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "payment_system_id")]
        public string PaymentSystemId { get; set; }
    }
}
