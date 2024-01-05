using System;

namespace IqSoft.CP.DAL.Models.Clients
{
    public class LimitItem
    {
        public decimal? Limit { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ChangeDate { get; set; }
        public string UpdateLimit { get; set; }
    }
}
