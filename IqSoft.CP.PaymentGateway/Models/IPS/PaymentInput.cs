using System;

namespace IqSoft.CP.PaymentGateway.Models.IPS.Payment
{
    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("proc-cc-result", Namespace = "", IsNullable = false)]
    public partial class PaymentInput
    {
        [System.Xml.Serialization.XmlElementAttribute("proc-cc-error")]
        public procccresultProcccerror procccerror { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("proc-cc-container")]
        public procccresultProccccontainer proccccontainer { get; set; }
    }

    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class procccresultProcccerror
    {
        [System.Xml.Serialization.XmlElementAttribute("proc-cc-code")]
        public byte proccccode { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("proc-cc-message")]
        public string procccmessage { get; set; }
    }

    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class procccresultProccccontainer
    {
        public int approved { get; set; }
        public string ordertype { get; set; }
        public int orderid { get; set; }
        public ushort internalorderid { get; set; }
        public string code { get; set; }
        public object descriptor { get; set; }
    }
}