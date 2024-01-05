using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.Wooppay
{
    [XmlType("response")]
    public class WooppayStartTransactionResult
    {
        [XmlElement("merchantId")]
        public string MerchantId { get; set; }

        [XmlElement("customerReference")]
        public string TransactionId { get; set; }
    }
}
