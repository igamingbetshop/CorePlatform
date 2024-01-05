using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.PayBox
{
    public class BaseOutput
    {
        [XmlElement("pg_payment_id")]
        public string pg_payment_id { get; set; }

        [XmlElement("pg_status")]
        public string pg_status { get; set; }

        [XmlElement("pg_redirect_url")]
        public string pg_redirect_url { get; set; }

        [XmlElement("pg_salt")]
        public string pg_salt { get; set; }

        [XmlElement("pg_sig")]
        public string pg_sig { get; set; }

        [XmlElement("pg_error_code")]
        public string pg_error_code { get; set; }

        [XmlElement("pg_error_description")]
        public string pg_error_description { get; set; }
    }
}
