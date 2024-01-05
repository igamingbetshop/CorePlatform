using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.PayBox
{
    [XmlType("request")]
    public class WithdrawRequestInput : BaseInput
    {
        [XmlElement("pg_user_id")]
        public string pg_user_id { get; set; }

        [XmlElement("pg_card_id_to")]
        public string pg_card_id_to { get; set; }

        [XmlElement("pg_post_link")]
        public string pg_post_link { get; set; }

        [XmlElement("pg_order_time_limit")]
        public string pg_order_time_limit { get; set; }

        [XmlElement("pg_phone")]
        public string pg_phone { get; set; }
    }
}
