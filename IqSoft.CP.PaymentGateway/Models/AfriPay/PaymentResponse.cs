namespace IqSoft.CP.PaymentGateway.Models.AfriPay
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("response", Namespace = "", IsNullable = false)]
    public class PaymentResponse
    {
        [System.Xml.Serialization.XmlElementAttribute("result")]
        public ResponseResult result { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ResponseResult
    {
        [System.Xml.Serialization.XmlElementAttribute("attribute")]
        public ResponseResultAttribute[] attribute { get; set; }
    
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int state { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int substate { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int code { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool final { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string trans { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string server_time { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ResponseResultAttribute
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value { get; set; }
    }
}