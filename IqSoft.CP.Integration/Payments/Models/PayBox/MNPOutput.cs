using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.PayBox
{
    public class MNPOutput
    {
        [XmlElement("pg_merchant_id")]
        public string pg_merchant_id { get; set; }

        [XmlElement("pg_operator_name")]
        public string pg_operator_name { get; set; }

        [XmlElement("pg_operator_code")]
        public string pg_operator_code { get; set; }

        [XmlElement("pg_status")]
        public string pg_status { get; set; }

        [XmlElement("pg_salt")]
        public string pg_salt { get; set; }

        [XmlElement("pg_sig")]
        public string pg_sig { get; set; }
    }
}