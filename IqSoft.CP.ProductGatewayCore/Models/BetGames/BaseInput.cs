using System;
using System.Xml.Serialization;

namespace IqSoft.CP.ProductGateway.Models.BetGames
{
    [Serializable]
    [XmlType("root")]
    public class BaseInput
    {
        [XmlElement("method")]
        public string Method { get; set; }

        [XmlElement("token")]
        public string Token { get; set; }

        [XmlElement("time")]
        public int Time { get; set; }

        [XmlElement("signature")]
        public string Signature { get; set; }
    }
}