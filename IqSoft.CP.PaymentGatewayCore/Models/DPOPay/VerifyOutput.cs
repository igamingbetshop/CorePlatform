
namespace IqSoft.CP.PaymentGateway.Models.DPOPay
{
    [System.Xml.Serialization.XmlRootAttribute("API3G", Namespace = "", IsNullable = false)]
    public class VerifyOutput
    {
        public string Result { get; set; }
        public string ResultExplanation { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCredit { get; set; }
        public string CustomerCreditType { get; set; }
        public string TransactionApproval { get; set; }
        public string TransactionCurrency { get; set; }
        public string TransactionAmount { get; set; }
        public string FraudAlert { get; set; }
        public string FraudExplnation { get; set; }
        public string TransactionNetAmount { get; set; }
        public string TransactionSettlementDate { get; set; }
        public string TransactionRollingReserveAmount { get; set; }
        public string TransactionRollingReserveDate { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerCountry { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerZip { get; set; }
        public string MobilePaymentRequest { get; set; }
        public string AccRef { get; set; }
        public string TransactionFinalCurrency { get; set; }
        public string TransactionFinalAmount { get; set; }
    }
}