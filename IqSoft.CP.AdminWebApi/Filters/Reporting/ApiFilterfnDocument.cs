using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterfnDocument : ApiFilterBase
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? ClientId { get; set; }
        public long? AccountId { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation ExternalTransactionIds { get; set; }
        public ApiFiltersOperation Amounts { get; set; }
        public ApiFiltersOperation States { get; set; }
        public ApiFiltersOperation OperationTypeIds { get; set; }
        public ApiFiltersOperation PaymentRequestIds { get; set; }
        public ApiFiltersOperation PaymentSystemIds { get; set; }
        public ApiFiltersOperation PaymentSystemNames { get; set; }
        public ApiFiltersOperation RoundIds { get; set; }
        public ApiFiltersOperation ProductIds { get; set; }
        public ApiFiltersOperation ProductNames { get; set; }
        public ApiFiltersOperation GameProviderIds { get; set; }
        public ApiFiltersOperation GameProviderNames { get; set; }
        public ApiFiltersOperation LastUpdateTimes { get; set; }
    }
}