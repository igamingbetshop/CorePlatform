using System;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterAccountsBalanceHistory
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public int ClientId { get; set; }

        public int UserId { get; set; }

        public long? AccountId { get; set; }
    }
}