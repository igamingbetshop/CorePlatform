using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class BalanceOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "Balance")]
        public decimal Balance { get; set; }
    }
}