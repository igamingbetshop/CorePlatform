namespace IqSoft.CP.PaymentGateway.Models.PerfectMoney
{
    public class PaymentRequestInput
    {
        public string PAYMENT_ID { get; set; }
        public string PAYEE_ACCOUNT { get; set; }
        public decimal PAYMENT_AMOUNT { get; set; }
        public string PAYMENT_UNITS { get; set; }
        public string PAYMENT_BATCH_NUM { get; set; }
        public string PAYER_ACCOUNT { get; set; }
        public string TIMESTAMPGMT { get; set; }
        public string V2_HASH { get; set; }
    }
}