using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Ezugi
{
    public class DebitInput : GeneralInput
    {
        [JsonProperty(PropertyName = "debitAmount")]
        public decimal DebitAmount { get; set; }

        [JsonProperty(PropertyName = "betTypeID")]
        public int BetTypeID { get; set; }
    }
}