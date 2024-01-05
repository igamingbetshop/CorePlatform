using System.Collections.Generic;
using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Products.Models.Betsoft
{
//    [XmlType(AnonymousType = true)]
//    [XmlRoot(Namespace = "", ElementName = "GAMESSUITES", IsNullable = false)]
//    public class  GamesOutput
//    {
//        [XmlArrayItemAttribute("SUITES", IsNullable = false)]
//        public List<GameSuites> SUITES { get; set; }
//    }

//    public class GameSuites
//    {
//        [XmlArrayItemAttribute("GAMES", IsNullable = false)]
//        public List<GameItem> GAMES { get; set; }
//        public string ID { get; set; }

//        public string NAME { get; set; }
//    }

//    public class GameItem
//    {
//        public int ID { get; set; }
//        public string NAME { get; set; }
//        public string IMAGEURL { get; set; }
//        public string LANGUAGES { get; set; }
//        public string CATEGORYID { get; set; }

//    }



[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[XmlTypeAttribute(AnonymousType = true)]
[XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class GAMESSUITES
{
    [XmlArrayItemAttribute("SUITE", IsNullable = false)]
    public List<GAMESSUITESSUITE> SUITES { get; set; }
}

[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class GAMESSUITESSUITE
{
    [XmlArrayItemAttribute("GAME", IsNullable = false)]
    public List<GAMESSUITESSUITEGAME> GAMES { get; set; }

    [XmlAttributeAttribute()]
    public string ID { get; set; }

    [XmlAttributeAttribute()]
    public string NAME { get; set; }
}

[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[XmlTypeAttribute(AnonymousType = true)]
public partial class GAMESSUITESSUITEGAME
{
    [XmlAttributeAttribute()]
    public ushort ID { get; set; }

    [XmlAttributeAttribute()]
    public string NAME { get; set; }

    [XmlAttributeAttribute()]
    public string IMAGEURL { get; set; }

    [XmlAttributeAttribute()]
    public string LANGUAGES { get; set; }

    [XmlAttributeAttribute()]
    public string CATEGORYID { get; set; }
}


}
