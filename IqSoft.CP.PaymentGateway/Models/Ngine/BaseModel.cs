namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class BaseModel
    {
        public object GetBalance { get; set; }
        public object PayoutCanceled { get; set; }
        public object Transfer { get; set; }
        public object PayoutRequest { get; set; }
        public object CustomerInfo { get; set; }
    }
}