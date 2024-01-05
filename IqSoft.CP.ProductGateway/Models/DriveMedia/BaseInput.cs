using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.DriveMedia
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "cmd")]
        public string Command { get; set; }

        [JsonProperty(PropertyName = "space")]
        public int Space { get; set; }

        [JsonProperty(PropertyName = "login")]
        public string[] Login { get; set; }

        [JsonProperty(PropertyName = "bet")]
        public long BetAmount { get; set; }

        [JsonProperty(PropertyName = "winLose")]
        public long WinLoseAmount { get; set; }

        [JsonProperty(PropertyName = "tradeId")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "betInfo")]
        public string BetInfo { get; set; }

        [JsonProperty(PropertyName = "gameId")]
        public string ExternalGameId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }

        [JsonProperty(PropertyName = "sign")]
        public string Sign { get; set; }

        [JsonProperty(PropertyName = "data")]
        public AdditionalData Data { get; set; }
    }

    public class AdditionalData
    {
        [JsonProperty(PropertyName = "winLines")]
        public string WinLines { get; set; }

        [JsonProperty(PropertyName ="time")]
        public string Time { get; set; }

        [JsonProperty(PropertyName = "ticketId")]
        public string TicketId { get; set; }

        [JsonProperty(PropertyName = "system")]
        public int? System { get; set; }

        [JsonProperty(PropertyName = "session")]
        public string Session { get; set; }

        [JsonProperty(PropertyName = "sequence")]
        public string Sequence { get; set; }

        [JsonProperty(PropertyName = "roundId")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "platform")]
        public string Platform { get; set; }

        [JsonProperty(PropertyName = "params")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "matrix")]
        public string Matrix { get; set; }

        [JsonProperty(PropertyName = "lines")]
        public string Lines { get; set; }

        [JsonProperty(PropertyName = "lang")]
        public string Lang { get; set; }

        [JsonProperty(PropertyName = "game_uuid")]
        public string GameUuid { get; set; }

        [JsonProperty(PropertyName = "freespin_win_sum")]
        public string FreespinWinSum { get; set; }

        [JsonProperty(PropertyName = "denomination")]
        public string Denomination { get; set; }

        [JsonProperty(PropertyName = "bet_type")]
        public string BetType { get; set; }

        [JsonProperty(PropertyName = "bet")]
        public string Bet { get; set; }
    }

    public class MatrixType
    {
        [JsonProperty(PropertyName ="type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName ="num")]
        public string Num { get; set; }

        [JsonProperty(PropertyName ="code")]
        public string Code { get; set; }
    }
}