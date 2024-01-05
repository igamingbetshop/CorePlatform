namespace IqSoft.CP.ProductGateway.Models.EveryMatrix
{
    public class TransactionOutput : BaseOutput
    {
        public TransactionOutput(BaseOutput b)
        {
            ApiVersion = b.ApiVersion;
            ReturnCode = b.ReturnCode;
            Request = b.Request;
            SessionId = b.SessionId;
            Message = b.Message;
        }
        public string AccountTransactionId { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }
        public decimal BonusMoneyAffected { get; set; }
        public decimal RealMoneyAffected { get; set; }
    }
}