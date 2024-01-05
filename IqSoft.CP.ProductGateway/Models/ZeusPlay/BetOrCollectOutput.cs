using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ZeusPlay
{
    public class BetOrCollectOutput
    {
        [JsonProperty(PropertyName = "random_number")]
        public int RandomNumber { get; set; }

        [JsonProperty(PropertyName = "player_balance_amount")]
        public decimal PlayerBalanceAmount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "datasig")]
        public string DataSignature { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
    }
}