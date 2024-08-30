using IqSoft.CP.Common.Models.Filters;
using System;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByObjectChangeHistory : ApiFilterBase
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? PartnerId { get; set; }
        public int? ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public ApiFiltersOperation Ids { get; set; }
        public ApiFiltersOperation ObjectIds { get; set; }
        public ApiFiltersOperation ObjectTypeIds { get; set; }
        public ApiFiltersOperation UserIds { get; set; }
    }
}