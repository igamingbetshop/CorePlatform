using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.SunCity
{
    public class BetOutput
    {
        public BetOutput()
        {
            Transactions = new List<TransactionOutput>();
        }

        [JsonProperty(PropertyName = "err")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errdesc")]
        public string ErrorDescription { get; set; }

        [JsonProperty(PropertyName = "transactions")]
        public List<TransactionOutput> Transactions { get; set; }
    }

    public class TransactionOutput
    {
        [JsonProperty(PropertyName = "txid")]
        public string txid { get; set; }

        [JsonProperty(PropertyName = "ptxid")]
        public string ptxid { get; set; }

        [JsonProperty(PropertyName = "bal")]
        public decimal bal { get; set; }

        [JsonProperty(PropertyName = "cur")]
        public string cur { get; set; }

        [JsonProperty(PropertyName = "dup")]
        public bool dup { get; set; }

        [JsonProperty(PropertyName = "err")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errdesc")]
        public string ErrorDescription { get; set; }
    }
}