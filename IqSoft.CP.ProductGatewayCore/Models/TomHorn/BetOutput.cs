using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TomHorn
{
    public class BetOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "Transaction")]
        public Transaction Transaction { get; set; }
    }

    public class Transaction 
    {
        [JsonProperty(PropertyName = "ID")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "Balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "Currency")]
        public string Currency { get; set; }
    }
}