using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Affiliates
{
    public class AffilkaActivityModel
    {
        [JsonProperty(PropertyName = "from")]
        public string FromDate { get; set; }

        [JsonProperty(PropertyName = "to")]
        public string ToDate { get; set; }

        [JsonProperty(PropertyName = "items")]
        public List<ActivityItem> Items { get; set; }
    }
    public class ActivityItem
    {

        [JsonProperty(PropertyName = "tag")]
        public string Tag { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "deposits")]
        public List<DepositModel> Deposits { get; set; }

        [JsonProperty(PropertyName = "deposits_sum_cents")]
        public int TotalDepositAmount { get; set; }

        [JsonProperty(PropertyName = "deposits_count")]
        public int TotalDepositCount { get; set; }

        [JsonProperty(PropertyName = "cashouts_sum_cents")]
        public int TotalWithdrawAmount { get; set; }

        [JsonProperty(PropertyName = "cashouts_count")]
        public int TotalWithdrawCount { get; set; }

        [JsonProperty(PropertyName = "bets_sum_cents")]
        public int TotalBetsInCents { get; set; }

        [JsonProperty(PropertyName = "wins_sum_cents")]
        public int TotalWinsInCents { get; set; }

        [JsonProperty(PropertyName = "casino_bets_count")]
        public int CasinoBetsCount { get; set; }

        [JsonProperty(PropertyName = "sb_bets_sum_cents")]
        public int SportTotalBetsInCents { get; set; }

        [JsonProperty(PropertyName = "sb_settled_bets_sum_cents")]
        public int SportTotalCaclculatedBetsInCents { get; set; }

        [JsonProperty(PropertyName = "sb_cancelled_bets_sum_cents")]
        public int SportCancelledBetsInCents { get; set; }

        [JsonProperty(PropertyName = "sb_rejected_bets_sum_cents")]
        public int SportRejectedBetsInCents { get; set; }

        [JsonProperty(PropertyName = "sb_wins_sum_cents")]
        public int SportTotalWinsInCent { get; set; }

        [JsonProperty(PropertyName = "bonus_issues_sum_cents")]
        public int BonusAmount { get; set; }

        [JsonProperty(PropertyName = "sb_balance_corrections_sum_cents")]
        public int CorrectionOnClient { get; set; }

        [JsonProperty(PropertyName = "chargebacks_sum_cents")]
        public int ChargeBack { get; set; }

        [JsonProperty(PropertyName = "chargebacks_count")]
        public int ChargeBackCount { get; set; }
    }

    public class DepositModel
    {
        [JsonProperty(PropertyName = "deposit_id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "amount_cents")]
        public int Amount { get; set; }

        [JsonProperty(PropertyName = "processed_at")]
        public string ProcessedAt { get; set; }
    }
}