using IqSoft.CP.Common.Models.Filters;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByClientExclusion : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation ClientIds { get; set; }
        public ApiFiltersOperation Usernames { get; set; }
        public ApiFiltersOperation DepositLimitDailys { get; set; }
        public ApiFiltersOperation DepositLimitWeeklys { get; set; }
        public ApiFiltersOperation DepositLimitMonthlys { get; set; }
        public ApiFiltersOperation TotalBetAmountLimitDailys { get; set; }
        public ApiFiltersOperation TotalBetAmountLimitWeeklys { get; set; }
        public ApiFiltersOperation TotalBetAmountLimitMonthlys { get; set; }
        public ApiFiltersOperation TotalLossLimitDailys { get; set; }
        public ApiFiltersOperation TotalLossLimitWeeklys { get; set; }
        public ApiFiltersOperation TotalLossLimitMonthlys { get; set; }
        public ApiFiltersOperation SystemDepositLimitDailys { get; set; }
        public ApiFiltersOperation SystemDepositLimitWeeklys { get; set; }
        public ApiFiltersOperation SystemDepositLimitMonthlys { get; set; }
        public ApiFiltersOperation SystemTotalBetAmountLimitDailys { get; set; }
        public ApiFiltersOperation SystemTotalBetAmountLimitWeeklys { get; set; }
        public ApiFiltersOperation SystemTotalBetAmountLimitMonthlys { get; set; }
        public ApiFiltersOperation SystemTotalLossLimitDailys { get; set; }
        public ApiFiltersOperation SystemTotalLossLimitWeeklys { get; set; }
        public ApiFiltersOperation SystemTotalLossLimitMonthlys { get; set; }
        public ApiFiltersOperation SessionLimits { get; set; }
        public ApiFiltersOperation SystemSessionLimits { get; set; }
    }
}