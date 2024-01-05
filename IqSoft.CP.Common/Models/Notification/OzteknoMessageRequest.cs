using System.Xml.Serialization;

namespace IqSoft.CP.Common.Models
{
    public class OzteknoMessageRequest
    {
        [XmlElement("BILGI")]
        public Info ServiceInfo { get; set; }

        [XmlElement("ISLEM")]
        public Process ProcessInfo { get; set; }
    }

    [XmlType("BILGI")]
    public class Info
    {
        [XmlElement("KULLANICI_ADI")]
        public string UserName { get; set; }

        [XmlElement("SIFRE")]
        public string Password { get; set; }

        [XmlElement("GONDERIM_TARIH")]
        public string SendDate { get; set; }
    }

    [XmlType("ISLEM")]
    public class Process
    {
        [XmlElement("YOLLA")]
        public Send SendInfo { get; set; }
    }

    [XmlType("YOLLA")]
    public class Send
    {
        [XmlElement("MESAJ")]
        public string MassageText { get; set; }

        [XmlElement("NO")]
        public string MobileNumber { get; set; }
    }
}
