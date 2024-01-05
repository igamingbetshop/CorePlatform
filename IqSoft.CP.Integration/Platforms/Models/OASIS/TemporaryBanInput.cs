using System;
using System.Reactive;

namespace IqSoft.CP.Integration.Platforms.Models.OASIS
{
    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.hzd.de/kurzzeitsperrdaten")]
    [System.Xml.Serialization.XmlRootAttribute("KURZZEITSPERRDATEN", Namespace = "http://www.hzd.de/kurzzeitsperrdaten", IsNullable = false)]
    public partial class TemporaryBanInput
    {
        [System.Xml.Serialization.XmlElementAttribute("SPIELER", Namespace = "")]
        public Player PlayerData { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("ZEITPUNKT_BETAETIGUNG_SCHALTFLAECHE", Namespace = "")]
        public Timestamp TimestampData { get; set; }
    }

    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("SPIELER", Namespace = "", IsNullable = false)]
    public partial class Player
    {
        [System.Xml.Serialization.XmlAttributeAttribute("V")]
        public string FirstName { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute("N")]
        public string Surname { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute("G")]
        public string BirthName { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("D")]
        public string BirthDate { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute("O")]
        public string BirthPlace { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute("A")]

        public AddressData Address { get; set; }
    }

    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute("SPIELERA", AnonymousType = true)]
    public partial class AddressData
    {
        [System.Xml.Serialization.XmlAttributeAttribute("P")]
        public string ZipCode { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute("W")]
        public string City { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute("S")]
        public string Street { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute("H")]
        public string HousNumber { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute("Z")]
        public string SupplementalAddress { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute("L")]
        public string CountryCode { get; set; }
    }

    [SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Timestamp
    {
        [System.Xml.Serialization.XmlAttributeAttribute("DATUM")]
        public string Date { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("FORMAT-PATTERN")]
        public string FormatPattern { get; set; }
    }
}
