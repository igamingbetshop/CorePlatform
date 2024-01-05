
using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.Help2Pay
{
    [XmlType("Payout")]
    public class PayoutOutput
    {
        [XmlElement("statusCode")]
        public string StatusCode { get; set; }
        [XmlElement("message")]
        public string Message { get; set; }
    }
}
