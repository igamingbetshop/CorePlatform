using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.SDPay
{
    [XmlType("TransferInfomation")]
    public class PayoutRequestInput
    {
        [XmlElement("Id")]
        public long Id { get; set; }

        /// <summary>
        /// Card Number (must be the same as the card number
        /// on the bank card; do not add spaces when entering
        /// the numbers) 
        /// </summary>
        [XmlElement("IntoAccount")]
        public string IntoAccount { get; set; }

        [XmlElement("IntoName")]
        public string IntoName { get; set; }

        [XmlElement("IntoBank1")]
        public string IntoBank1 { get; set; }

        [XmlElement("IntoBank2")]
        public string IntoBank2 { get; set; }

        [XmlElement("IntoAmount")]
        public decimal Amount { get; set; }

        [XmlElement("SerialNumber")]
        public string SerialNumber { get; set; }
    }
}
