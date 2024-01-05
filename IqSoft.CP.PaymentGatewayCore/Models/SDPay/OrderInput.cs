using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.SDPay
{
    [XmlType("message")]
    public class OrderInput
    {
        [XmlElement("cmd")]
        public string Command { get; set; }

        [XmlElement("merchantid")]
        public string MerchantId { get; set; }

        [XmlElement("order")]
        public int TransactionId { get; set; }

        [XmlElement("username")]
        public string ClientId { get; set; }

        [XmlElement("money")]
        public decimal Amount { get; set; }

        [XmlElement("unit")]
        public int Currency { get; set; }

        [XmlElement("time")]
        public string Time { get; set; }

        [XmlElement("call")]
        public string Call { get; set; }

        [XmlElement("result")]
        public int ResponseCode { get; set; }

        [XmlElement("remark")]
        public string Remark { get; set; }
    }
}