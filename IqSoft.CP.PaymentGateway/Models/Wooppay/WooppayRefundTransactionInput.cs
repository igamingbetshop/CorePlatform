using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.Wooppay
{
    [XmlType("request")]
    public class WooppayRefundTransactionInput : WooppayCompleteTransactionInput
    {
        [XmlElement("refundAmount")]
        public decimal Amount { get; set; }

        [XmlElement("password")]
        public string Password { get; set; }

        [XmlElement("currencyCode")]
        public int CurrencyCode { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }
    }
}
