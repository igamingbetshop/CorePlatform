using IqSoft.CP.Common.Models.Filters;
using System;
namespace IqSoft.CP.AdminWebApi.Filters.Reporting 
{
    public class ApiFilterReportByBonus : ApiFilterBase
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation BonusIds { get; set; }
        public ApiFiltersOperation BonusNames { get; set; }
        public ApiFiltersOperation BonusTypes { get; set; }
        public ApiFiltersOperation BonusStatuses { get; set; }
        public ApiFiltersOperation ClientIds { get; set; }
        public ApiFiltersOperation UserNames { get; set; }
        public ApiFiltersOperation FirstNames { get; set; }
        public ApiFiltersOperation LastNames { get; set; }
        public ApiFiltersOperation CategoryIds { get; set; }
        public ApiFiltersOperation CurrencyIds { get; set; }
        public ApiFiltersOperation Emails { get; set; }
        public ApiFiltersOperation MobileNumbers { get; set; }
        public ApiFiltersOperation BonusPrizes { get; set; }
        public ApiFiltersOperation SpinsCounts { get; set; }
        public ApiFiltersOperation TurnoverAmountLefts { get; set; }
        public ApiFiltersOperation RemainingCredits { get; set; }
        public ApiFiltersOperation WageringTargets { get; set; }
        public ApiFiltersOperation FinalAmounts { get; set; }
        public ApiFiltersOperation Statuses { get; set; }
        public ApiFiltersOperation AwardingTimes { get; set; }
        public ApiFiltersOperation CreationTimes { get; set; }
        public ApiFiltersOperation CalculationTimes { get; set; }
        public ApiFiltersOperation ValidUntils { get; set; }
    }
}