using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ZeusPlay
{
    public class GameSessionEndOutput
    {
        [JsonProperty(PropertyName = "game_session_id")]
        public string GameSessionId { get; set; }

        [JsonProperty(PropertyName = "datasig")]
        public string DataSignature { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
    }
}