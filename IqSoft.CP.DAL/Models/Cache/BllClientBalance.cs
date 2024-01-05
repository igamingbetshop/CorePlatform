
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllClientBalance
    {
        public long ClientId { get; set; }
        public string CurrencyId { get; set; }
        public decimal AvailableBalance { get; set; }
        public List<BllClientAccount> Balances { get; set; }
    }

    [Serializable]
    public class BllClientAccount
    {
        public long Id { get; set; }
        public int TypeId { get; set; }
        public string CurrencyId { get; set; }
        public decimal Balance { get; set; }
        public int? BetShopId { get; set; }
        public int? PaymentSystemId { get; set; }
    }
}