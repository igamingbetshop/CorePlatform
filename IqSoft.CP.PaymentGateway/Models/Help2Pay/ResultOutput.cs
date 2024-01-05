using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.Help2Pay
{
    [XmlRoot("Payout")]
    public class ResultOutput
    {
        [XmlElement("statusCode")]
        public string StatusCode { get; set; }

        [XmlElement("message")]
        public string Message { get; set; }
    }
}