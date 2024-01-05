using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllTicker
    {
        public string Message { get; set; }
        public List<int> ClientIds { get; set; }
        public List<int> UserIds { get; set; }
        public List<int> SegmentIds { get; set; }
    }
}
