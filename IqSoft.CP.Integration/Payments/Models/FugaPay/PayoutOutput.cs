

using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.FugaPay
{
   public class PayoutOutput: OutputBase
    {
        [JsonProperty(PropertyName = "response")]
        public PayoutResult Result{ get; set; }
    }

    public class PayoutResult
    {
        [JsonProperty(PropertyName = "WithdrawRequestId")]
        public string WithdrawRequestId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}
