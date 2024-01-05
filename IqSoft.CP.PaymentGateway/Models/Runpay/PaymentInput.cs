namespace IqSoft.CP.PaymentGateway.Models.Runpay
{
    public class PaymentInput
    {
        public string LMI_MERCHANT_ID { get; set; }
        public string LMI_PAYMENT_NO { get; set; }
        public string LMI_SYS_PAYMENT_ID { get; set; }
        public string LMI_SYS_PAYMENT_DATE { get; set; }
        public string LMI_PAYMENT_AMOUNT { get; set; }
        public string LMI_CURRENCY { get; set; }
        public string LMI_PAID_AMOUNT { get; set; }
        public string LMI_PAID_CURRENCY { get; set; }
        public string LMI_PAYMENT_METHOD { get; set; }
        public string LMI_SIM_MODE { get; set; }
        public string LMI_PAYMENT_DESC { get; set; }
        public string LMI_HASH { get; set; }
        public string LMI_PAYER_IDENTIFIER { get; set; }
        public string LMI_PAYER_COUNTRY { get; set; }
        public string LMI_PAYER_PASSPORT_COUNTRY { get; set; }
        public string LMI_PAYER_IP_ADDRESS { get; set; }
        public string LMI_PAYMENT_SYSTEM { get; set; }
    }
}