using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.MicroGaming
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", ElementName = "pkt", IsNullable = false)]
    public class Input
    {
        [XmlElement(ElementName = "methodcall")]
        public MethodCall MethodCall { get; set; }
    }
    
    [XmlTypeAttribute(AnonymousType = true)]
    public class MethodCall
    {
        [XmlElement(ElementName = "auth")]
        public MethodCallAuth Auth { get; set; }

        [XmlElement(ElementName = "call")]
        public MethodCallCall Call { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "timestamp")]
        public string TimeStamp { get; set; }

        [XmlAttribute(AttributeName = "system")]
        public string System { get; set; }
    }
    
    [XmlTypeAttribute(AnonymousType = true)]
    public class MethodCallAuth
    {
        [XmlAttribute(AttributeName = "login")]
        public string Login { get; set; }

        [XmlAttribute(AttributeName = "password")]
        public string Password { get; set; }
    }
    
    [XmlType(AnonymousType = true)]
    public class MethodCallCall
    {
        [XmlElement(ElementName = "extinfo")]
        public object ExtInfo { get; set; }

        [XmlAttribute(AttributeName = "seq")]
        public string Seq { get; set; }

        [XmlAttribute(AttributeName = "token")]
        public string Token { get; set; }

        [XmlAttribute(AttributeName = "playtype")]
        public string PlayType { get; set; }

        [XmlAttribute(AttributeName = "gameid")]
        public string GameId { get; set; }

        [XmlAttribute(AttributeName = "gamereference")]
        public string GameReference { get; set; }

        [XmlAttribute(AttributeName = "actionid")]
        public string ActionId { get; set; }

        [XmlAttribute(AttributeName = "actiondesc")]
        public string ActionDesc { get; set; }

        [XmlAttribute(AttributeName = "amount")]
        public int Amount { get; set; }

        [XmlAttribute(AttributeName = "start")]
        public bool Start { get; set; }

        [XmlAttribute(AttributeName = "finish")]
        public bool Finish { get; set; }

        [XmlAttribute(AttributeName = "offline")]
        public bool Offline { get; set; }

        [XmlAttribute(AttributeName = "currency")]
        public string Currency { get; set; }

        [XmlAttribute(AttributeName = "freegame")]
        public string FreeGame { get; set; }
    }
}