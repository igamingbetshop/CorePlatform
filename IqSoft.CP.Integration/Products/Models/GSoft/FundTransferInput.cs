using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    class FundTransferInput : BaseInput
    {
        [JsonProperty(PropertyName = "OpTransId")]
        public long TransactionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "Direction")]
        public int Direction { get; set; }
    }
}