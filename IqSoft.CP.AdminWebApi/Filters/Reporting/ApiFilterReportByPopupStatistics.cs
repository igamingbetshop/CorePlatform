using System;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByPopupStatistics : ApiFilterBase
    {
        public int? PopupId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation PartnerIds { get; set; }
        public ApiFiltersOperation NickNames { get; set; }
        public ApiFiltersOperation Types { get; set; }
        public ApiFiltersOperation DeviceTypes { get; set; }
        public ApiFiltersOperation States { get; set; }
        public ApiFiltersOperation CreationTimes { get; set; }
        public ApiFiltersOperation LastUpdateTimes { get; set; }
        public ApiFiltersOperation ViewTypeIds { get; set; }
        public ApiFiltersOperation ViewCounts { get; set; }
    }
}