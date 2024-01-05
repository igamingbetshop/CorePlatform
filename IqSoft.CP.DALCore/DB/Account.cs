﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class Account
    {
        public Account()
        {
            AccountBalances = new HashSet<AccountBalance>();
            AccountClosedPeriods = new HashSet<AccountClosedPeriod>();
            Transactions = new HashSet<Transaction>();
        }

        public long Id { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public int TypeId { get; set; }
        public decimal Balance { get; set; }
        public string CurrencyId { get; set; }
        public long? SessionId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public virtual Currency Currency { get; set; }
        public virtual ObjectType ObjectType { get; set; }
        public virtual AccountType Type { get; set; }
        public virtual ICollection<AccountBalance> AccountBalances { get; set; }
        public virtual ICollection<AccountClosedPeriod> AccountClosedPeriods { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}