using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.SDPay
{
    [XmlType("message")]
    public class ResultOutput
    {
        [XmlElement("cmd")]
        public string Command { get; set; }

        [XmlElement("merchantid")]
        public string MerchantId { get; set; }

        [XmlElement("username")]
        public string ClientId { get; set; }

        [XmlElement("order")]
        public string TransactionId { get; set; }

        [XmlElement("result")]
        public int ResponseCode { get; set; }
    }
}