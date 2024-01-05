using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.PayBox
{
    [XmlRoot("response")]
    public class AddCardResultInput
    {
        public string pg_type { get; set; }

        public long pg_payment_id { get; set; }

        public long pg_order_id { get; set; }

        public long pg_card_3ds { get; set; }

        public string pg_card_hash { get; set; }

        public string pg_card_hhash { get; set; }

        public int pg_card_month { get; set; }

        public string pg_card_year { get; set; }

        public string pg_bank { get; set; }

        public string pg_country { get; set; }

        public long pg_recurring_profile_id { get; set; }

        public long pg_card_id { get; set; }

        public string pg_user_id { get; set; }

        public string pg_status { get; set; }

        public string pg_salt { get; set; }

        public string pg_sig { get; set; }
    }

    public class XmlInput
    {
        public string pg_xml { get; set; }
    }

    class CardInfo
    {
        public string pg_card_id { get; set; }

        public string pg_card_number { get; set; }
    }

}