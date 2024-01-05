using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ZeusPlay
{
    public class GameSessionValidOutput
    {
        [JsonProperty("player_id")]
        public int PlayerId { get; set; }

        [JsonProperty("game_id")]
        public int GameId { get; set; }

        [JsonProperty("datasig")]
        public string DataSignature { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
    }
}