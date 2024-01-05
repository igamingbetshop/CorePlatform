using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.Kassa24
{
    [XmlRoot("response")]
    public class ResponseBase
    {
        [XmlElement("code")]
        public int Code { get; set; }

        [XmlElement("message")]
        public string Message { get; set; }
    }
}
