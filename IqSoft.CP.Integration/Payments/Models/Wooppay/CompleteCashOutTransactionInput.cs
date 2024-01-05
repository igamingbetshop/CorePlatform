using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.Wooppay
{
     [XmlType("request")]
    public class CompleteCashOutTransactionInput
    {
        [XmlElement("merchantId")]
        public string MerchantId { get; set; }

        [XmlElement("merchantKeyword")]
        public string MerchantKeyword { get; set; }

        [XmlElement("referenceNr")]
        public long TransactionId { get; set; }

        [XmlElement("transactionSuccess")]
        public bool TransactionSuccess { get; set; }
    }
}
