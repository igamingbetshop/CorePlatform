using IqSoft.CP.DAL;

namespace IqSoft.CP.ProductGateway.Models.SmartSoft
{
    public class TransactionInput
    {
        public string TransactionId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public Transaction TransactionInfo { get; set; }
    }

    public class Transaction
    {
        public string Source { get; set; }
        public string GameNumber { get; set; }
        public string RoundId { get; set; }
        public string GameName { get; set; }

    }
}