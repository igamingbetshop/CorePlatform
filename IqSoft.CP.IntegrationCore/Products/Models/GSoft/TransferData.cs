using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class TransferData
    {
        [JsonProperty(PropertyName = "trans_id")]
        public long TransactionExternalId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "before_amount")]
        public decimal BeforeAmount { get; set; }

        [JsonProperty(PropertyName = "after_amount")]
        public decimal AfterAmount { get; set; }

        [JsonProperty(PropertyName = "transfer_date")]
        public DateTime TransferDate { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public int Currency { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }
    }
}