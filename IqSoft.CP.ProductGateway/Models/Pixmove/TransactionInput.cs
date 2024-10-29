namespace IqSoft.CP.ProductGateway.Models.Pixmove
{
    public class TransactionInput : BaseInput
    {
        public string amount { get; set; } 
        public string roundId { get; set; }
        public string transactionId { get; set; }
        public string betTransactionId { get; set; }
    }
}