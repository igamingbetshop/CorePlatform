using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterfnClientDashboard : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ApiFiltersOperation ClientIds { get; set; }
        public ApiFiltersOperation UserNames { get; set; }
        public ApiFiltersOperation Emails { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation CurrencyIds { get; set; }
        public ApiFiltersOperation FirstNames { get; set; }
        public ApiFiltersOperation LastNames { get; set; }
        public ApiFiltersOperation AffiliatePlatformIds { get; set; }
        public ApiFiltersOperation AffiliateIds { get; set; }
        public ApiFiltersOperation AffiliateReferralIds { get; set; }
        public ApiFiltersOperation TotalWithdrawalAmounts { get; set; }
        public ApiFiltersOperation WithdrawalsCounts { get; set; }
        public ApiFiltersOperation TotalDepositAmounts { get; set; }
        public ApiFiltersOperation DepositsCounts { get; set; }
        public ApiFiltersOperation TotalBetAmounts { get; set; }
        public ApiFiltersOperation TotalBetsCounts { get; set; }
        public ApiFiltersOperation SportBetsCounts { get; set; }
        public ApiFiltersOperation TotalWinAmounts { get; set; }
        public ApiFiltersOperation WinsCounts { get; set; }
        public ApiFiltersOperation GGRs { get; set; }
        public ApiFiltersOperation NGRs { get; set; }
        public ApiFiltersOperation TotalDebitCorrections { get; set; }
        public ApiFiltersOperation DebitCorrectionsCounts { get; set; }
        public ApiFiltersOperation TotalCreditCorrections { get; set; }
        public ApiFiltersOperation CreditCorrectionsCounts { get; set; }
        public ApiFiltersOperation ComplementaryBalances { get; set; }
    }
}