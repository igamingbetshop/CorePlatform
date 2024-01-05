using System;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.TwoWinPower
{
    [XmlType(AnonymousType = false, Namespace = "urn:2winpower:api:seamless")]
    [XmlRoot(Namespace = "urn:2winpower:api:seamless", ElementName = "session", IsNullable = false)]
    [Serializable]
    public class SessionOutput : BaseOutput
    {
        [XmlElement(ElementName = "username")]
        public long UserName { get; set; }

        [XmlElement(ElementName = "currency")]
        public string CurrencyId { get; set; }

        [XmlElement(ElementName = "game")]
        public string Game { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }
}