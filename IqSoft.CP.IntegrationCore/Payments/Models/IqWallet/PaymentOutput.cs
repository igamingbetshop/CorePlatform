namespace IqSoft.CP.Integration.Payments.Models.IqWallet
{
    public class PaymentOutput
    {
        public int Status { get; set; }

        public long MerchantPaymentId { get; set; }

        public long PaymentId { get; set; }

        public string Sign { get; set; }
        
        public int ErrorCode { get; set; }

        public string ErrorDescription { get; set; }
    }
}
