using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Report
{
    public class PartnerReport
    {

        [JsonProperty(PropertyName = "New Players")]
        public int NewPlayers { get; set; }

        [JsonProperty(PropertyName = "Active Players")]
        public int ActivePlayers { get; set; }

        [JsonProperty(PropertyName = "Withdrawable Balance")]
        public decimal WithdrawableBalance { get; set; }

        [JsonProperty(PropertyName = "Bonus Conversion")]
        public decimal BonusConversion { get; set; }
    }
    public class PartnerGameReport
    {

        [JsonProperty(PropertyName = "Game")]
        public List<GameProviderReportItem> GameProvider { get; set; }
    }

    public class PartnerWithdrawalsReport
    {
        [JsonProperty(PropertyName = "Withdrawals")]
        public List<WithdrawPaymentSystemReportItem> Withdrawals { get; set; }

        public List<TotalWithdrawalResult> TotalWithdrawalAmount { get; set; }
    }

    public class PartnerDepositsReport
    {
        [JsonProperty(PropertyName = "Deposits")]
        public List<DepositPaymentSystemReportItem> Deposits { get; set; }

        public List<TotalDepositResult> TotalDepositAmount { get; set; }
    }
    public class TotalDepositResult
    {
        private const string val= "Total Deposit Amount";
        [JsonProperty(PropertyName = "")]
        public string Name { get; set; } = val;

        [JsonProperty(PropertyName = "")]
        public decimal TotalAmount { get; set; }
    }

    public class TotalWithdrawalResult
    {
        private const string val = "Total Withdrawal Amount";
        [JsonProperty(PropertyName = "")]
        public string Name { get; set; } = val;

        [JsonProperty(PropertyName = "")]
        public decimal TotalAmount { get; set; }
    }

    public class GameProviderReportItem
    {
        [JsonProperty(PropertyName = "Provider Name")]
        public string GameProviderName { get; set; }

        [JsonProperty(PropertyName = "Total Bet Amount")]
        public decimal TotalBetAmount { get; set; }

        [JsonProperty(PropertyName = "Total Win Amount")]
        public decimal TotalWinAmount { get; set; }

        [JsonProperty(PropertyName = "GGR")]
        public decimal GGR { get; set; }
    }

    public class GameProviderTotalItem
    {
        [JsonProperty(PropertyName = "Total Bet Amount")]
        public decimal TotalBetAmount { get; set; }

        [JsonProperty(PropertyName = "Total Win Amount")]
        public decimal TotalWinAmount { get; set; }

        [JsonProperty(PropertyName = "Total GGR")]
        public decimal TotalGGR { get; set; }
    }
}
