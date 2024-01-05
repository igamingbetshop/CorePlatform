using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.Igromat
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class service
    {
        [XmlElementAttribute("enter")]
        public serviceEnter[] enter { get; set; }

        [XmlElementAttribute("getbalance")]
        public serviceGetbalance[] getbalance { get; set; }

        [XmlElementAttribute("roundbet")]
        public serviceRoundbet[] roundbet { get; set; }

        [XmlElementAttribute("roundwin")]
        public serviceRoundwin roundwin { get; set; }

        [XmlElementAttribute("refund")]
        public serviceRefund refund { get; set; }

        [XmlElementAttribute("logout")]
        public serviceLogout logout { get; set; }

        [XmlAttribute]
        public string session { get; set; }

        [XmlAttribute]
        public string time { get; set; }
    }

    #region enter

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class serviceEnter
    {
        [XmlElement("user")]
        public serviceEnterUser user { get; set; }

        [XmlElement("balance")]
        public serviceBalance balance { get; set; }

        [XmlElement("control")]
        public serviceEnterControl[] control { get; set; }

        [XmlAttribute]
        public ulong id { get; set; }

        [XmlAttribute]
        public string result { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class serviceEnterUser
    {
        [XmlAttribute]
        public string wlid { get; set; }

        [XmlAttribute]
        public string mode { get; set; }

        [XmlAttribute]
        public string type { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class serviceEnterControl
    {
        [XmlAttribute]
        public string stream { get; set; }

        [XmlAttribute]
        public string enable { get; set; }
    }

    #endregion

    #region getbalance

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serviceGetbalance
    {
        [XmlElement("balance")]
        public serviceBalance balance { get; set; }

        [XmlAttributeAttribute]
        public ulong id { get; set; }

        [XmlAttributeAttribute]
        public string result { get; set; }
    }

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serviceBalance
    {
        [XmlAttributeAttribute]
        public decimal value { get; set; }

        [XmlAttributeAttribute]
        public int version { get; set; }

        [XmlAttributeAttribute]
        public string type { get; set; }

        [XmlAttributeAttribute]
        public string currency { get; set; }
    }

    #endregion

    #region roundbet

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serviceRoundbet
    {
        [XmlElement("balance")]
        public serviceBalance balance { get; set; }

        [XmlAttributeAttribute]
        public int id { get; set; }

        [XmlAttributeAttribute]
        public string result { get; set; }
    }

    #endregion

    #region roundwin

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serviceRoundwin
    {
        [XmlElement("balance")]
        public serviceBalance balance { get; set; }

        [XmlAttributeAttribute]
        public ulong id { get; set; }

        [XmlAttributeAttribute]
        public string result { get; set; }
    }

    #endregion

    #region refund

    [Serializable]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serviceRefund
    {
        [XmlAttribute]
        public int id { get; set; }

        [XmlAttribute]
        public string result { get; set; }

        [XmlElement("balance")]
        public serviceBalance balance { get; set; }
    }

    #endregion

    #region logout

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serviceLogout
    {
        [XmlElement("balance")]
        public serviceLogoutBalance balance { get; set; }

        [XmlAttributeAttribute]
        public ulong id { get; set; }

        [XmlAttributeAttribute]
        public string result { get; set; }
    }

    [SerializableAttribute]
    [DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public class serviceLogoutBalance
    {
        [XmlAttributeAttribute]
        public decimal value { get; set; }

        [XmlAttributeAttribute]
        public byte version { get; set; }

        [XmlAttributeAttribute]
        public string type { get; set; }

        [XmlAttributeAttribute]
        public string currency { get; set; }
    }

    #endregion
}