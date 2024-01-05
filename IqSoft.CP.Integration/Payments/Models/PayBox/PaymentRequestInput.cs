using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.PayBox
{
    [XmlType("request")]
    public class PaymentRequestInput : BaseInput
    {
        [XmlElement("pg_currency")]
        public string pg_currency { get; set; }

        [XmlElement("pg_abonent_phone")]
        public string pg_abonent_phone { get; set; }

        [XmlElement("pg_user_id")]
        public string pg_user_id { get; set; }

        [XmlElement("pg_payment_system")]
        public string pg_payment_system { get; set; }

        [XmlElement("pg_success_url")]
        public string pg_success_url { get; set; }

        [XmlElement("pg_failure_url")]
        public string pg_failure_url { get; set; }

        [XmlElement("pg_recurring_start")]
        public int pg_recurring_start { get; set; }

        [XmlElement("pg_recurring_lifetime")]
        public int pg_recurring_lifetime { get; set; }

        [XmlElement("pg_result_url")]
        public string pg_result_url { get; set; }
        
        [XmlElement("pg_check_url")]
        public string pg_check_url { get; set; }

    }
}
