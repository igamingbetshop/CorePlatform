using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Cartipal
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "withdrawal_id")]
        public string WithdrawalId { get; set; }

        [JsonProperty(PropertyName = "errorDescription")]
        public string ErrorDescription { get; set; }
    }
}
