using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.TwoWinPower
{
    [XmlType(AnonymousType = true, Namespace = "urn:2winpower:api:seamless")]
    [XmlRoot(Namespace = "urn:2winpower:api:seamless", ElementName = "error", IsNullable = false)]
    public class ErrorOutput : BaseOutput
    {
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        [XmlElement(ElementName = "description")]
        public string Description { get; set; }
    }
}