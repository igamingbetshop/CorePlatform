using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.Wooppay
{
    [XmlType("response")]
    public class WooppayTransactionStatus
    {
        [XmlElement("transactionSuccess")]
        public string TransactionSuccess { get; set; }
    }

    public static class TransactionStatus
    {
        public const string NoSuchTransaction = "NO_SUCH_TRANSACTION";

        public const string PendingCustomerInput = "PENDING_CUSTOMER_INPUT";

        public const string PendingAuthResult = "PENDING_AUTH_RESULT";

        public const string Authorised = "AUTHORISED";

        public const string Declined = "DECLINED";

        public const string Reversed = "REVERSED";

        public const string Paid = "PAID";

        public const string Refunded = "REFUNDED";

        public const string InvalidMid = "INVALID_MID";

        public const string MidDisabled = "MID_DISABLED";
    }
}