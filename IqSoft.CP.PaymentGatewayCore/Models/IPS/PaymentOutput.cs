namespace IqSoft.CP.PaymentGateway.Models.IPS
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("proc-cc-result", Namespace = "", IsNullable = false)]
    public partial class procccresult
    {
        [System.Xml.Serialization.XmlElementAttribute("proc-cc-error")]
        public procccresultProcccerror procccerror { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("proc-cc-container")]
        public procccresultProccccontainer proccccontainer { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class procccresultProcccerror
    {
        [System.Xml.Serialization.XmlElementAttribute("proc-cc-code")]
        public byte proccccode { get; set; }


        [System.Xml.Serialization.XmlElementAttribute("proc-cc-message")]
        public string procccmessage { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class procccresultProccccontainer
    {
        public int approved { get; set; }
        public string ordertype { get; set; }
        public int orderid { get; set; }
        public int internalorderid { get; set; }
        public string code { get; set; }
        public string descriptor { get; set; }
        public procccresultProccccontainerRedirection redirection { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class procccresultProccccontainerRedirection
    {
        public procccresultProccccontainerRedirectionUrl url { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("var", IsNullable = false)]
        public procccresultProccccontainerRedirectionVar[] vars { get; set; }
    }
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class procccresultProccccontainerRedirectionUrl
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type { get; set; }
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class procccresultProccccontainerRedirectionVar
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string attr { get; set; }

        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value { get; set; }
    }
}