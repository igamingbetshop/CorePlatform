using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterAccountsBalanceHistory : ApiFilterBase
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public int ClientId { get; set; }

        public int UserId { get; set; }

        public long? AccountId { get; set; }
    }
}