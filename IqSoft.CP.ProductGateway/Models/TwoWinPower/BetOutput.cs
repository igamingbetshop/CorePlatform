using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.TwoWinPower
{
    [XmlType(AnonymousType = true, Namespace = "urn:2winpower:api:seamless")]
    [XmlRoot(Namespace = "urn:2winpower:api:seamless", ElementName = "transaction", IsNullable = false)]
    public class BetOutput : BaseOutput
    {
        [XmlElement(ElementName = "currency")]
        public string CurrencyId { get; set; }

        [XmlAttribute(AttributeName = "externalId")]
        public long ExternalId { get; set; }

        [XmlAttribute(AttributeName = "sessionId")]
        public string SessionId { get; set; }
    }
}