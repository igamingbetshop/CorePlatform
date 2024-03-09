using IqSoft.CP.DAL.Models.Clients;
using System;
using System.Activities.Debugger;

namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class ApiClientLimit
    {
        public LimitItem DepositLimitDaily { get; set; }
        public LimitItem DepositLimitWeekly { get; set; }
        public LimitItem DepositLimitMonthly { get; set; }
        public LimitItem TotalBetAmountLimitDaily { get; set; }
        public LimitItem TotalBetAmountLimitWeekly { get; set; }
        public LimitItem TotalBetAmountLimitMonthly { get; set; }
        public LimitItem TotalLossLimitDaily { get; set; }
        public LimitItem TotalLossLimitWeekly { get; set; }
        public LimitItem TotalLossLimitMonthly { get; set; }
        public int? SessionLimit { get; set; }
        public int? SessionLimitDaily { get; set; }
        public int? SessionLimitWeekly { get; set; }
        public int? SessionLimitMonthly { get; set; }
        public int? SelfExclusionPeriod { get; set; }
        public decimal? DefaultDepositLimitMonthly { get; set; }
        public decimal? DefaultUnverifiedDepositLimitMonthly { get; set; }
        public decimal? DefaultLossPercentMonthly { get; set; }
        public decimal? MinDepositLimitMonthly { get; set; }
        public decimal? MaxDepositLimitMonthly { get; set; }
        public decimal? MinUnverifiedDepositLimitMonthly { get; set; }
        public bool IsCreditVerified { get; set; }
        public bool IsHighCreditVerified { get; set; }

        public SelfExcludedModel SelfExcluded { get; set; }
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
        public SelfExcludedModel SystemExcluded { get; set; }
    }

    public class SelfExcludedModel
    {
        public DateTime? ExcludedUntil { get; set; }
        public int Reason { get; set; }
        public string ReasonText { get; set; }
    }
}