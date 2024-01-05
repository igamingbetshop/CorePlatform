namespace IqSoft.CP.ProductGateway.Models.WinSystems
{
    public class TransactionInput : BaseInput
    {
        public string CurrencyId { get; set; }
        public string ClientId { get; set; }
        public string TransactionId { get; set; }
        public string CreditTransactionId { get; set; }
        public string BetId { get; set; }
        public string RollbackTransactionId { get; set; }
        public string RoundId { get; set; }
        public string Amount { get; set; }
        public int? OperationTypeId { get; set; }
    }
}