using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.GMW
{
    public class TransactionInput : BaseInput
    {
        public string TransactionId { get; set; }
        public List<TransactionItem> Transactions { get; set; }
    }

    public class TransactionItem
    {
        public string Type { get; set; }
        public decimal Amount { get; set; }
    }
}