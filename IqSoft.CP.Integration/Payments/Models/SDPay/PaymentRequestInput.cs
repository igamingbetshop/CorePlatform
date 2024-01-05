using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.SDPay
{
    [XmlType("message")]
    public class OrderData
    {
        [XmlElement("cmd")]
        public string Command { get; set; }

        [XmlElement("merchantid")]
        public string MerchantId { get; set; }

        [XmlElement("language")]
        public string Language { get; set; }

        [XmlElement("userinfo")]
        public UserInfo User { get; set; }
    }


    public class UserInfo
    {
        [XmlElement("order")]
        public string OrderId { get; set; }

        [XmlElement("username")]
        public string ClientId { get; set; }

        [XmlElement("money")]
        public decimal Amount { get; set; }

        [XmlElement("unit")]
        public int Currency { get; set; }

        [XmlElement("remark")]
        public string Remark { get; set; }

        [XmlElement("time")]
        public string Time { get; set; }

        [XmlElement("backurl")]
        public string BackUrl { get; set; }

        [XmlElement("backurlbrowser")]
        public string BackurlBrowser { get; set; }
    }
}
