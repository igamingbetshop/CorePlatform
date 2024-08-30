using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Filters.PaymentRequests
{
    public class ApiFilterReportByPaymentSystem : ApiFilterBase
    {
        public int? PartnerId { get; set; }
        public int Type { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation PaymentSystemIds { get; set; }
        public ApiFiltersOperation PaymentSystemNames { get; set; }
        public ApiFiltersOperation Statuses { get; set; }
        public ApiFiltersOperation Counts { get; set; }
        public ApiFiltersOperation TotalAmounts { get; set; }
    }
}