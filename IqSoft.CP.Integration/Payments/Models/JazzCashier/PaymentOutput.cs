namespace IqSoft.CP.Integration.Payments.Models.JazzCashier
{
    public class PaymentOutput
    { 
        public int Code { get; set; }
        public string Message { get; set; }
        public string IdTypeCode { get; set; }
        public PaymentData Data { get; set; }
        public bool NeedConfirmation { get; set; }
    }

    public class PaymentData
    {
        public string IdInternalTransaction { get; set; }
        public string IdReference { get; set; }
        public int IdTransactionType { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentSystemUrl { get; set; }
    }
}
