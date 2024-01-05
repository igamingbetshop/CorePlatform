using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.PayBox
{
    [XmlRoot("response")]
    public class WithdrawResultInput
    {
        public string pg_status { get; set; }

        public string pg_merchant_id { get; set; }

        public string pg_payment_id { get; set; }

        public string pg_order_id { get; set; }

        public string pg_balance { get; set; }

        public string pg_payment_amount { get; set; }

        public string pg_payment_date { get; set; }

        public string pg_card_id { get; set; }

        public string pg_card_hash { get; set; }

        public string pg_salt { get; set; }

        public string pg_sig { get; set; }
    }
}