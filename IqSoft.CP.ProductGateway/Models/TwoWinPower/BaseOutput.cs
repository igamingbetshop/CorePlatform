using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TwoWinPower
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "balance")]
        public decimal? Balance { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "error_description")]
        public string ErrorDescription { get; set; }
    }
}