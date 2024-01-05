using Newtonsoft.Json;
namespace IqSoft.CP.ProductGateway.Models.DragonGaming
{
    public class BetInput : BaseInput
    {
        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

        [JsonProperty(PropertyName = "amount_type")]
        public string AmountType { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "game_id")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "round_id")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "bonus_id")]
        public string BonusId { get; set; }

        [JsonProperty(PropertyName = "round_end_state")]
        public bool RoundEndState { get; set; }

        [JsonProperty(PropertyName = "original_transaction_id")]
        public string OriginalTransactionId { get; set; }
    }
}