using System;

namespace IqSoft.CP.AgentWebApi.Filters
{
    public class ApiFilterDashboard
    {
        public int? PartnerId { get; set; }

        public int? ProductId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}