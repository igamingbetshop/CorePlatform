using System;
using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.Kassa24
{
    [XmlRoot("response")]
    public class Response 
    {
        [XmlElement("code")]
        public int Code { get; set; }

        [XmlElement("message")]
        public string Message { get; set; }

        [XmlElement("date")]
        public DateTime Date { get; set; }

        [XmlElement( "authcode")]
        public int Authcode { get; set; }
    }
}