namespace IqSoft.CP.ProductGateway.Models.EveryMatrix
{
    public class BalanceOutput : BaseOutput
    {
        public BalanceOutput(BaseOutput b)
        {
            ApiVersion = b.ApiVersion;
            ReturnCode = b.ReturnCode;
            Request = b.Request;
            SessionId = b.SessionId;
            Message = b.Message;
        }
        public decimal Balance { get; set; }
        public decimal BonusMoney { get; set; }
        public decimal RealMoney { get; set; }
        public string Currency { get; set; }
        public object AdditionalData { get; set; }
    }
}