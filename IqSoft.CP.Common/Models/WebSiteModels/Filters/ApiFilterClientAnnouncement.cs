using System;

namespace IqSoft.CP.Common.Models.WebSiteModels.Filters
{
    public class ApiFilterClientAnnouncement : ApiFilterBase
    {
        public int Type { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
