using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.PayBox
{
    public class BaseInput
    {
        [XmlElement("pg_merchant_id")]
        public string pg_merchant_id { get; set; }

        [XmlElement("pg_order_id")]
        public string pg_order_id { get; set; }

        [XmlElement("pg_payment_route")]
        public string pg_payment_route { get; set; }

        [XmlElement("pg_amount")]
        public string pg_amount { get; set; }

        [XmlElement("pg_description")]
        public string pg_description { get; set; }
        
        [XmlElement("pg_sig")]
        public string pg_sig { get; set; }

        [XmlElement("pg_salt")]
        public string pg_salt { get; set; }
    }
}
