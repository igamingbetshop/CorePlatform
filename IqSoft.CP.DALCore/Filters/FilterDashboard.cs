using System;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterDashboard
    {
        public int? PartnerId { get; set; }

        public int? ProductId { get; set; }

        public int? AgentId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}
