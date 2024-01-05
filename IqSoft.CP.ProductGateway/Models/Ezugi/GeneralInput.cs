using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.Ezugi
{
    public class GeneralInput : AuthenticationInput
    {
        [JsonProperty(PropertyName = "serverId")]
        public int ServerId { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public int ClientId { get; set; }

        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "roundId")]
        public long RoundId { get; set; }

        [JsonProperty(PropertyName = "gameId")]
        public int GameId { get; set; }

        [JsonProperty(PropertyName = "tableId")]
        public int TableId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "seatId")]
        public string SeatId { get; set; }
    }
}