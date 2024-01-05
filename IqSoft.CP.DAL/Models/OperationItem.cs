namespace IqSoft.CP.DAL.Models
{
    public class OperationItem
    {
        public decimal Amount { get; set; }

        public int OperationTypeId { get; set; }

        public int Type { get; set; }

        public int ObjectId { get; set; }

        public int ObjectTypeId { get; set; }

        public string CurrencyId { get; set; }

        public int? AccountTypeId { get; set; }

        public long? AccountId { get; set; }
        public int? BetShopId { get; set; }

    }
}
