using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.Wooppay
{
    [XmlType("response")]
   public class CompleteCashOutTransactionOutput
    {
        [XmlElement("transactionStatus")]
        public string TransactionStatus { get; set; }

        [XmlElement("authCode")]
        public string AuthCode { get; set; }

        [XmlElement("cardIssuerCountry")]
        public string CardIssuerCountry { get; set; }

        [XmlElement("maskedCardNumber")]
        public string MaskedCardNumber { get; set; }

        [XmlElement("userIpAddress")]
        public string UserIpAddress { get; set; }

        [XmlElement("success")]
        public bool Success { get; set; }

        [XmlElement("errorDescription ")]
        public string ErrorDescription { get; set; }
    }
}
