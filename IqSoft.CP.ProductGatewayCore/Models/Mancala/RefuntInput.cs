namespace IqSoft.CP.ProductGateway.Models.Mancala
{
    public class RefuntInput :BaseInput
    {
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public string RefundTransactionGuid { get; set; }
        public string RoundGuid { get; set; }
    }
}