namespace IqSoft.CP.PaymentGateway.Models.Ecopayz
{
    public partial class SVSPurchaseStatusNotificationResponse
    {
        public SVSPurchaseStatusNotificationResponseTransactionResult TransactionResult { get; set; }
        public string Status { get; set; }
        public SVSPurchaseStatusNotificationResponseAuthentication Authentication { get; set; }
    }

    public partial class SVSPurchaseStatusNotificationResponseTransactionResult
    {
        public string Description { get; set; }
        public int Code { get; set; }
    }

    public partial class SVSPurchaseStatusNotificationResponseAuthentication
    {
        public string Checksum { get; set; }
    }
}