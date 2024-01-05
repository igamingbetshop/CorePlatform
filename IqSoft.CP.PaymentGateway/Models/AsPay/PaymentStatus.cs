namespace IqSoft.CP.PaymentGateway.Models.AsPay
{
    public class PaymentStatus
    {
        public decimal Amount { get; set; }
        public string TrxId { get; set; }
        public int Status { get; set; }
    }
}