using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.SoftSwiss
{
    public class BaseInput
    {
        [JsonProperty(PropertyName = "user_id")]
        public int ClientId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "game")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "game_id")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "finished")]
        public bool Finished { get; set; }
        
        [JsonProperty(PropertyName = "actions")]
        public List<Action> Actions { get; set; }

        [JsonProperty(PropertyName = "session_id")]
        public string SessionId { get; set; }

    }

    public class Action
    {
        [JsonProperty(PropertyName = "action")]
        public string ActionName { get; set; }

        [JsonProperty(PropertyName = "action_id")]
        public string ActionId { get; set; }

        [JsonProperty(PropertyName = "original_action_id")]
        public string OriginalActionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "jackpot_contribution")]
        public decimal? JackpotContribution { get; set; }

        [JsonProperty(PropertyName = "jackpot_win")]
        public decimal? JackpotWin { get; set; }
    }
}