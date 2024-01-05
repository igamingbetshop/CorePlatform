using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.User
{
    public class UserBalance
    {
        public int UserId { get; set; }
        public int ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Nickname { get; set; }
        public string LastLoginIp { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int Status { get; set; }
        public string CurrencyId { get; set; }
        public decimal Credit { get; set; }
        public decimal AvailableCredit { get; set; }
        public decimal Cash { get; set; }
        public decimal AvailableCash { get; set; }
        public decimal YesterdayCash { get; set; }
        public decimal Outstanding { get; set; }
        public decimal ParentAvailableBalance { get; set; }
        public decimal? AgentMaxCredit { get; set; }

        //   public List<UserAccount> Balances { get; set; }
    }
    public class UserAccount
    {
        public string CurrencyId { get; set; }
        public decimal Balance { get; set; }
        public decimal? Credit { get; set; }
    }
}
