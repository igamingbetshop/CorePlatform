using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.Igromat
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class server
    {
        [XmlElement("enter")]
        public serverEnter[] enter { get; set; }

        [XmlElement("getbalance")]
        public serverGetbalance[] getbalance { get; set; }

        [XmlElementAttribute("roundbet")]
        public serverRoundbet[] roundbet { get; set; }

        [XmlElementAttribute("roundwin")]
        public serverRoundwin roundwin { get; set; }

        [XmlElementAttribute("refund")]
        public serverRefund refund { get; set; }

        [XmlElementAttribute("logout")]
        public serverLogout logout { get; set; }

        [XmlAttribute]
        public string session { get; set; }

        [XmlAttribute]
        public string time { get; set; }
    }

    #region enter

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class serverEnter
    {
        [XmlElement("game")]
        public serverEnterGame game { get; set; }

        [XmlAttribute]
        public ulong id { get; set; }

        [XmlAttribute]
        public string guid { get; set; }

        [XmlAttribute]
        public string key { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class serverEnterGame
    {
        [XmlAttribute]
        public string name { get; set; }
    }

    #endregion

    #region getbalance

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class serverGetbalance
    {
        [XmlAttribute]
        public ulong id { get; set; }

        [XmlAttribute]
        public string guid { get; set; }
    }

    #endregion

    #region roundbet

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serverRoundbet
    {
        [XmlElement("roundnum")]
        public serverRoundbetRoundnum roundnum { get; set; }

        [XmlAttributeAttribute]
        public int id { get; set; }

        [XmlAttributeAttribute]
        public string guid { get; set; }

        [XmlAttributeAttribute]
        public int bet { get; set; }

        [XmlAttributeAttribute]
        public string type { get; set; }
    }

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serverRoundbetRoundnum
    {
        [XmlAttributeAttribute]
        public long id { get; set; }
    }

    #endregion

    #region roundwin

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serverRoundwin
    {
        [XmlElement("roundnum")]
        public serverRoundwinRoundnum roundnum { get; set; }

        [XmlElement("event")]
        public serverRoundwinEvent @event { get; set; }

        [XmlAttributeAttribute]
        public ulong id { get; set; }

        [XmlAttributeAttribute]
        public string guid { get; set; }

        [XmlAttributeAttribute]
        public uint win { get; set; }

        [XmlAttribute]
        public string type { get; set; }
    }

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serverRoundwinRoundnum
    {
        [XmlAttribute]
        public ulong id { get; set; }
    }

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serverRoundwinEvent
    {
        [XmlElement("info")]
        public serverRoundwinEventInfo info { get; set; }

        [XmlElementAttribute("data")]
        public serverRoundwinEventData[] data { get; set; }

        [XmlElementAttribute("combo")]
        public serverRoundwinEventCombo[] combo { get; set; }

        [XmlAttribute]
        public string type { get; set; }
    }

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serverRoundwinEventInfo
    {
        [XmlAttribute]
        public string type { get; set; }

        [XmlAttribute]
        public int num { get; set; }
    }

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serverRoundwinEventData
    {
        [XmlAttributeAttribute]
        public string type { get; set; }

        [XmlAttributeAttribute]
        public int num { get; set; }

        [XmlAttributeAttribute]
        public string code { get; set; }
    }

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serverRoundwinEventCombo
    {
        [XmlAttributeAttribute]
        public string type { get; set; }

        [XmlAttributeAttribute]
        public int num { get; set; }

        [XmlIgnoreAttribute]
        public bool numSpecified { get; set; }

        [XmlAttributeAttribute]
        public string code { get; set; }

        [XmlAttributeAttribute]
        public int cash { get; set; }

        [XmlIgnoreAttribute]
        public bool cashSpecified { get; set; }
    }

    #endregion

    #region refund

    [Serializable]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serverRefund
    {
        [XmlElement("storno")]
        public serverStorno storno { get; set; }

        [XmlAttribute]
        public int id { get; set; }

        [XmlAttribute]
        public string guid { get; set; }

        [XmlAttribute]
        public int cash { get; set; }
    }

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serverStorno
    {
        [XmlAttribute]
        public string cmd { get; set; }

        [XmlAttribute]
        public ulong id { get; set; }

        [XmlAttribute]
        public int cash { get; set; }

        [XmlAttribute]
        public int wlid { get; set; }

        [XmlAttribute]
        public string guid { get; set; }

        [XmlAttribute]
        public string gameid { get; set; }

        [XmlElement("roundnum")]
        public serverStornoRoundnum roundnum { get; set; }
    }

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serverStornoRoundnum
    {
        [XmlAttribute]
        public uint id { get; set; }
    }

    #endregion

    #region logout

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serverLogout
    {
        [XmlAttributeAttribute]
        public ulong id { get; set; }

        [XmlAttributeAttribute]
        public string guid { get; set; }

        [XmlElement("getbalance")]
        public serverGetbalance getbalance { get; set; }
    }

    #endregion
}

