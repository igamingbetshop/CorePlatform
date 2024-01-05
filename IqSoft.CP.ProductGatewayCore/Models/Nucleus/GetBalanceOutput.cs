using System;

namespace IqSoft.CP.ProductGateway.Models.Nucleus.Balance
{
    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class EXTSYSTEM
    {
        public EXTSYSTEMREQUEST REQUEST { get; set; }
        public string TIME { get; set; }
        public EXTSYSTEMRESPONSE RESPONSE { get; set; }
    }

    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EXTSYSTEMREQUEST
    {
        public string USERID { get; set; }
    }

    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EXTSYSTEMRESPONSE
    {
        public string RESULT { get; set; }
        public int BALANCE { get; set; }
    }

    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ERROREXTSYSTEM
    {
        public EXTSYSTEMREQUEST REQUEST { get; set; }
        public string TIME { get; set; }
        public ERRORRESPONSE RESPONSE { get; set; }
    }


    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ERRORRESPONSE
    {
        public string RESULT { get; set; }
        public int CODE { get; set; }
    }
}