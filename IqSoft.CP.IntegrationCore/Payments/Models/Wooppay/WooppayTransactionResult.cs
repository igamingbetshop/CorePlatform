using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.Wooppay
{
    [XmlType("response")]
    public class WooppayTransactionResult
    {
        [XmlElement("Success")]
        public bool Success { get; set; }

        [XmlElement("redirectURL")]
        public string RedirectURL { get; set; }

        [XmlElement("errorDescription")]
        public string ErrorDescription { get; set; }

        [XmlElement("customerReference")]
        public long TransactionId { get; set; }
    }
}
