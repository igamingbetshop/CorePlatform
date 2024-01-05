using System;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterReportBySegment
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int PartnerId { get; set; }
        public int PaymentSystemId { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
    }
}
