using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.YSB
{
    [Serializable]
    [XmlRoot(ElementName = "request")]
    public class ValidationInput
    {       
        [XmlElement(ElementName = "element")]
        public Element Elem { get; set; }
        [XmlAttribute(AttributeName = "action")]
        public string Action { get; set; }
    }

    [XmlRoot(ElementName = "element")]
    public class Element
    {
        public Element()
        {
            Properties = new List<Property>();
            Records = new List<Record>();
        }
        [XmlElement(ElementName = "properties")]
        public List<Property> Properties { get; set; }

        [XmlElement(ElementName = "Record")]
        public List<Record> Records { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "Record")]
    public class Record
    {
        [XmlElement(ElementName = "properties")]
        public List<Property> Properties { get; set; }
    }

        [XmlRoot(ElementName = "properties")]
    public class Property
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}