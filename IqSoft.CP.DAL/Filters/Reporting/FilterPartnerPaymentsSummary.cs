using System;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterPartnerPaymentsSummary
    {
        public int PartnerId { get; set; }

        public int Type { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}