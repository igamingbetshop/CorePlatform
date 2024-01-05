using Newtonsoft.Json;
namespace IqSoft.CP.ProductGateway.Models.AleaPlay
{
    public class BetInput : BaseInput
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "integratorTransactionId")]
        public string IntegratorTransactionId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "game")]
        public GameModel Game { get; set; }

        [JsonProperty(PropertyName = "player")]
        public PlayerModel Player { get; set; }

        [JsonProperty(PropertyName = "round")]
        public RoundModel Round { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "bet")]
        public BetModel Bet { get; set; }

        [JsonProperty(PropertyName = "win")]
        public BetModel Win { get; set; }

        [JsonProperty(PropertyName = "transaction")]
        public TransactionModel Transaction { get; set; }
    }


    public class TransactionModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }


    public class GameModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }

    public class BetModel
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }
    }

    public class PlayerModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "casinoPlayerId")]
        public string CasinoPlayerId { get; set; }
    }

    public class RoundModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}