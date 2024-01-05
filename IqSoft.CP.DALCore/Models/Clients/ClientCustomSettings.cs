namespace IqSoft.CP.DAL.Models.Clients
{
    public class ClientCustomSettings
    {
        public int ClientId { get; set; }
        public bool? AllowAutoPT { get; set; }
        public bool? AllowOutright { get; set; }
        public bool? AllowDoubleCommission { get; set; }
        public int? ParentState { get; set; }
        public decimal? MaxCredit { get; set; }
        public long? PasswordChangedDate { get; set; }
        public string TermsConditionsAcceptanceVersion { get; set; }
        public decimal? DepositLimitDaily { get; set; }
        public decimal? DepositLimitWeekly { get; set; }
        public decimal? DepositLimitMonthly { get; set; }
        public decimal? TotalBetAmountLimitDaily { get; set; }
        public decimal? TotalBetAmountLimitWeekly { get; set; }
        public decimal? TotalBetAmountLimitMonthly { get; set; }
        public decimal? TotalLossLimitDaily { get; set; }
        public decimal? TotalLossLimitWeekly { get; set; }
        public decimal? TotalLossLimitMonthly { get; set; }
        public int? SessionLimit { get; set; }
        public decimal? SystemDepositLimitDaily { get; set; }
        public decimal? SystemDepositLimitWeekly { get; set; }
        public decimal? SystemDepositLimitMonthly { get; set; }
        public decimal? SystemTotalBetAmountLimitDaily { get; set; }
        public decimal? SystemTotalBetAmountLimitWeekly { get; set; }
        public decimal? SystemTotalBetAmountLimitMonthly { get; set; }
        public decimal? SystemTotalLossLimitDaily { get; set; }
        public decimal? SystemTotalLossLimitWeekly { get; set; }
        public decimal? SystemTotalLossLimitMonthly { get; set; }
        public int? SystemSessionLimit { get; set; }
        public int? SelfExclusionPeriod { get; set; }
        public int? ReferralType { get; set; }

    }
}