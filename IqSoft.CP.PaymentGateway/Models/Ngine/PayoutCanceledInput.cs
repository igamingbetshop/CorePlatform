namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class PayoutCanceled
    {
        public string CustPIN { get; set; }
        public string CustPassword { get; set; }
        public string Amount { get; set; }
        public string ProcessorName { get; set; }
        public string TransactionID { get; set; }
        public string TransDate { get; set; }
        public string TransNote { get; set; }
        public string IPAddress { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class PayoutCanceledInput
    {
        public PayoutCanceled PayoutCanceled { get; set; }
    }
}