
using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Cartipal
{
    public class PayoutInput
    {
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "withdrawal_id")]
        public string WithdrawalId { get; set; }
        
        [JsonProperty(PropertyName = "reference")]
        public long PaymentRquestId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "errorDescription")]
        public string ErrorDescription { get; set; }
    }
}