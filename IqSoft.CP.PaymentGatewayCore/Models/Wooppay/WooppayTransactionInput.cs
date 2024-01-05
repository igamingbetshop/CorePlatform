using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.Wooppay
{
    [XmlType("request")]
    public class WooppayTransactionInput
    {
        [XmlElement("merchantId")]
        public string MerchantId { get; set; }

        [XmlElement("referenceNr")]
        public string TransactionId { get; set; }
    }
}