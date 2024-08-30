using System;

namespace IqSoft.CP.Common.Models.Filters
{
    public class ApiFilterfnAgentTransaction : ApiFilterBase
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int AffiliateId { get; set; }
        public string UserIdentity { get; set; }
        public int? UserState { get; set; }
        public bool? IsYesterday { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation FromUserIds { get; set; }
        public ApiFiltersOperation UserIds { get; set; }
        public ApiFiltersOperation ExternalTransactionIds { get; set; }
        public ApiFiltersOperation Amounts { get; set; }
        public ApiFiltersOperation CurrencyIds { get; set; }
        public ApiFiltersOperation States { get; set; }
        public ApiFiltersOperation OperationTypeIds { get; set; }
        public ApiFiltersOperation ProductIds { get; set; }
        public ApiFiltersOperation ProductNames { get; set; }
        public ApiFiltersOperation TransactionTypes { get; set; }
        public ApiFiltersOperation CreationTimes { get; set; }
        public ApiFiltersOperation LastUpdateTimes { get; set; }
    }
}