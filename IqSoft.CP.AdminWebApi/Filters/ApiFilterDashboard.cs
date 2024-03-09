using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterDashboard
    {
        public double TimeZone { get; set; }

        public int? PartnerId { get; set; }

        public int? ProductId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}