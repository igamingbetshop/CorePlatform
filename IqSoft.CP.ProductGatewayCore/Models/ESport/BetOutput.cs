using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.ESport
{
    public class BetOutput
    {
        public BetOutput()
        {
            Results = new List<BetOutputItem>();
        }
        [JsonProperty(PropertyName = "results")]
        public List<BetOutputItem> Results { get; set; }

        [JsonProperty(PropertyName = "error")]
        public int Error { get; set; }

        [JsonProperty(PropertyName = "error_description")]
        public string ErrorDescription { get; set; }
    }

    public class BetOutputItem
    {
        [JsonProperty(PropertyName = "wallet_id")]
        public string Wallet_Id { get; set; }

        [JsonProperty(PropertyName = "result")]
        public bool Result { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string Transaction_Id { get; set; }
    }
}