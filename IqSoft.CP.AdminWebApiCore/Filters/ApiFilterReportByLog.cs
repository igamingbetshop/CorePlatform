using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterReportByLog : ApiFilterBase
    {
        public int? Id { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}