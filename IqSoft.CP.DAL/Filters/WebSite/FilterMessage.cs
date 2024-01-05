using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Filters.WebSite
{
    public class FilterMessage
    {
        public int SkipCount { get; set; }

        public int TakeCount { get; set; }

        public int ClientId { get; set; }

        public List<int> MessageTypes { get; set; }
        
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}
