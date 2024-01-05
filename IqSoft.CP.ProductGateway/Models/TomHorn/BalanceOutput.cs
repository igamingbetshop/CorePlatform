using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TomHorn
{
    public class BalanceOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "Balance")]
        public Balance Balance { get; set; }
    }

    public class Balance
    {
        [JsonProperty(PropertyName = "Amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "Currency")]
        public string Currency { get; set; }
    }
}