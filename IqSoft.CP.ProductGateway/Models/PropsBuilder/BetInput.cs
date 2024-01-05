using Newtonsoft.Json;
using System;

namespace IqSoft.CP.ProductGateway.Models.PropsBuilder
{
    public class BetInput : BaseInput
    {
        [JsonProperty(PropertyName = "txnId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "betId")]
        public string BetId { get; set; }

        [JsonProperty(PropertyName = "txnDate")]
        public DateTime TransactionDate { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "odds")]
        public decimal Odds { get; set; }

        [JsonProperty(PropertyName = "returnAmount")]
        public decimal ReturnAmount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

       [JsonProperty(PropertyName = "originalTxnId")]
        public string OriginalTxnId { get; set; }
    }
}
