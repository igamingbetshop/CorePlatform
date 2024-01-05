using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models
{
    public class ObjectBalance
    {
        public long ObjectId { get; set; }

        public int ObjectTypeId { get; set; }

        public string CurrencyId { get; set; }

        public decimal AvailableBalance { get; set; }

        public IEnumerable<ObjectAccount> Balances { get; set; }
    }

    public class ObjectAccount
    {
        public long Id { get; set; }

        public int TypeId { get; set; }

        public decimal Balance { get; set; }

        public string CurrencyId { get; set; }
    }
}
