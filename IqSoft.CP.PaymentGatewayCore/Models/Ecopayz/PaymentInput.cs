namespace IqSoft.CP.PaymentGateway.Models.Ecopayz
{
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class TransactionResult
    {
        public ushort ErrorCode { get; set; }
        public string SvsTxID { get; set; }
        public string ProcessingTime { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Message { get; set; }
        public string MerchantAccountNumber { get; set; }
        public string ClientAccountNumber { get; set; }
        public string ClientAccountCurrency { get; set; }
        public string TransactionDescription { get; set; }
        public long ClientTransactionID { get; set; }
        public string UserToken { get; set; }
    }

    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class SVSPurchaseStatusNotificationRequest
    {
        public SVSPurchaseStatusNotificationRequestStatusReport StatusReport { get; set; }
        public SVSPurchaseStatusNotificationRequestRequest Request { get; set; }
        public SVSPurchaseStatusNotificationRequestAuthentication Authentication { get; set; }
    }

    public partial class SVSPurchaseStatusNotificationRequestStatusReport
    {
        public string StatusDescription { get; set; }
        public int Status { get; set; }
        public SVSPurchaseStatusNotificationRequestStatusReportSVSTransaction SVSTransaction { get; set; }
        public SVSPurchaseStatusNotificationRequestStatusReportSVSCustomer SVSCustomer { get; set; }
    }

    public partial class SVSPurchaseStatusNotificationRequestStatusReportSVSTransaction
    {
        public string SVSCustomerAccount { get; set; }
        public string ProcessingTime { get; set; }
        public SVSPurchaseStatusNotificationRequestStatusReportSVSTransactionResult Result { get; set; }
        public string BatchNumber { get; set; }
    }

    public partial class SVSPurchaseStatusNotificationRequestStatusReportSVSTransactionResult
    {
        public object Description { get; set; }
        public object Code { get; set; }
    }

    public partial class SVSPurchaseStatusNotificationRequestStatusReportSVSCustomer
    {
        public string IP { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
    }

    public partial class SVSPurchaseStatusNotificationRequestRequest
    {
        public string MerchantFreeText { get; set; }
        public string CustomerIdAtMerchant { get; set; }
        public string MerchantAccountNumber { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string TxBatchNumber { get; set; }
        public long TxID { get; set; }
        public string UserToken { get; set; }
    }

    public partial class SVSPurchaseStatusNotificationRequestAuthentication
    {
        public string Checksum { get; set; }
    }
}