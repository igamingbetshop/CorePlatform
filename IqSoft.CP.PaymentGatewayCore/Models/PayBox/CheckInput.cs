namespace IqSoft.CP.PaymentGateway.Models.PayBox
{
    public class CheckInput
    {
        public string pg_order_id { get; set; }

        public string pg_payment_id { get; set; }

        public decimal pg_amount { get; set; }

        public string pg_currency { get; set; }

        public decimal pg_ps_amount { get; set; }

        public decimal pg_ps_full_amount { get; set; }

        public string pg_ps_currency { get; set; }

        public string pg_payment_system { get; set; }

        public string pg_user_id { get; set; }

        public string pg_salt { get; set; }

        public string pg_sig { get; set; }
    }
}