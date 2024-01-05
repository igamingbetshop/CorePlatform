namespace IqSoft.CP.Integration.Payments.Models.AsPay
{
    public class PaymentOutput
    {
        public bool IsSuccess { get; set; }
        public string TransactionGuid { get; set; }
        public string RedirectUrl { get; set; }
        public string Message { get; set; }
    }
}
