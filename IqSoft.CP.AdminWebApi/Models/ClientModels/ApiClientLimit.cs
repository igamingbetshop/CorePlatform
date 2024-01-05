using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiClientLimit
    {
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
        public int? SessionLimitDaily { get; set; }
        public int? SessionLimitWeekly { get; set; }
        public int? SessionLimitMonthly { get; set; }
        public DateTime? SelfExcludedUntil { get; set; }

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
        public int? SystemSessionLimitDaily { get; set; }
        public int? SystemSessionLimitWeekly { get; set; }
        public int? SystemSessionLimitMonthly { get; set; }
        public DateTime? SystemExcludedUntil { get; set; }
        public int? SelfExclusionPeriod { get; set; }

    }
}