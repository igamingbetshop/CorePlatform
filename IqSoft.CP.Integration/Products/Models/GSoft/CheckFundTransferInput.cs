using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class CheckFundTransferInput //: BaseInput
    {
        [JsonProperty(PropertyName = "OpCode")]
        public string OpCode { get; set; }

        [JsonProperty(PropertyName = "PlayerName")]
        public string PlayerName { get; set; }

        [JsonProperty(PropertyName = "SecurityToken")]
        public string SecurityToken { get; set; }

        [JsonProperty(PropertyName = "OpTransId")]
        public long TransactionId { get; set; }
    }
}