using IqSoft.CP.ProductGateway.Models.ISoftBet;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.ESport
{
    public class BetInput
    {
        [JsonProperty(PropertyName = "data")]
        public List<BetItem> Data { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }
    }

    public class BetItem
    {
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "amount")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "bet_id")]
        public string Bet_Id { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string User_Id { get; set; }

        [JsonProperty(PropertyName = "wallet_id")]
        public string Wallet_Id { get; set; }
    }
}