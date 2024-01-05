using System;

namespace IqSoft.CP.Integration.Products.Models.Nucleus
{
    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class GAMESSUITES
    {
        [System.Xml.Serialization.XmlArrayItemAttribute("SUITE", IsNullable = false)]
        public GAMESSUITESSUITE[] SUITES { get; set; }
    }

    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GAMESSUITESSUITE
    {
        [System.Xml.Serialization.XmlArrayItemAttribute("GAME", IsNullable = false)]
        public GAMESSUITESSUITEGAME[] GAMES { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NAME { get; set; }
    }

    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GAMESSUITESSUITEGAME
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NAME { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IMAGEURL { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LANGUAGES { get; set; }

        public string CATEGORYNAME { get; set; }
    }
}
