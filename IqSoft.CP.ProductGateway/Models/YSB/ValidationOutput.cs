using System;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.YSB
{
    [Serializable]
    [XmlRoot(ElementName = "response")]
    public class ValidationOutput
    {
        public ValidationOutput()
        {
            Elem = new Element();
        }
        [XmlElement(ElementName = "element")]
        public Element Elem { get; set; }
        [XmlAttribute(AttributeName = "action")]
        public string Action { get; set; }
    }    
}