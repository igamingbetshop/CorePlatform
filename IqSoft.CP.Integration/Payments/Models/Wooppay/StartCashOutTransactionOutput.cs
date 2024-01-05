using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.Wooppay
{
    [XmlType("response")]
    public class StartCashOutTransactionOutput
    {
        [XmlElement("Success")]
        public bool Success { get; set; }

        /// <summary>
        /// 00 – успешно,остальные не успешные
        /// </summary>
        [XmlElement("rspCode")]
        public string RspCode { get; set; }

        [XmlElement("errorDescription")]
        public string ErrorDescription { get; set; }

        [XmlElement("customerReference")]
        public long TransactionId { get; set; }
    }
}
