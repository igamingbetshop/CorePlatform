namespace IqSoft.CP.PaymentGateway.Models.Ecopayz.Voucher
{
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class SVSPurchaseStatusNotificationRequest
    {
        public SVSPurchaseStatusNotificationRequestStatusReport StatusReport { get; set; }
        public SVSPurchaseStatusNotificationRequestRequest Request { get; set; }
        public SVSPurchaseStatusNotificationRequestAuthentication Authentication { get; set; }
    }

    public class SVSPurchaseStatusNotificationRequestStatusReport
    {
        public object StatusDescription { get; set; }
        public byte Status { get; set; }
        public SVSPurchaseStatusNotificationRequestStatusReportSVSTransaction SVSTransaction { get; set; }
        public SVSPurchaseStatusNotificationRequestStatusReportSVSCustomer SVSCustomer { get; set; }
    }

    public class SVSPurchaseStatusNotificationRequestStatusReportSVSTransaction
    {
        public uint SVSCustomerAccount { get; set; }
        public string ProcessingTime { get; set; }
        public SVSPurchaseStatusNotificationRequestStatusReportSVSTransactionResult Result { get; set; }
        public string BatchNumber { get; set; }
        public string Id { get; set; }
    }
    public class SVSPurchaseStatusNotificationRequestStatusReportSVSTransactionResult
    {
        public object Description { get; set; }
        public object Code { get; set; }
    }
    public class SVSPurchaseStatusNotificationRequestStatusReportSVSCustomer
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
        public uint TxID { get; set; }
    }

    public class SVSPurchaseStatusNotificationRequestAuthentication
    {
        public string Checksum { get; set; }
    }
}