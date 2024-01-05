using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ZeusPlay
{
    public class PlayerBalanceQueryOutput
    {
        [JsonProperty(PropertyName = "game_session_id")]
        public string GameSessionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "datasig")]
        public string DataSignature { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
    }
}