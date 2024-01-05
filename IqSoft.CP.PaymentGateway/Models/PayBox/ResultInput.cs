namespace IqSoft.CP.PaymentGateway.Models.PayBox
{
    public class ResultInput
    {
        public string pg_order_id { get; set; }

        public string pg_payment_id { get; set; }

        public string pg_amount { get; set; }

        public string pg_net_amount { get; set; }

        public string pg_ps_amount { get; set; }

        public string pg_ps_full_amount { get; set; }

        public string pg_currency { get; set; }

        public string pg_ps_currency { get; set; }

        public string pg_payment_system { get; set; }

        public string pg_description { get; set; }

        public string pg_result { get; set; }

        public string pg_can_reject { get; set; }

        public string pg_user_phone { get; set; }

        public string pg_card_brand { get; set; }

        public string pg_auth_code { get; set; }

        public string pg_salt { get; set; }

        public string pg_sig { get; set; }

        public string pg_user_id { get; set; }

        public string pg_card_id { get; set; }

        public string pg_captured { get; set; }

        public string pg_overpayment { get; set; }

        public string pg_failure_code { get; set; }

        public string pg_failure_description { get; set; }

        public int? pg_recurring_profile_id { get; set; }

        public string pg_recurring_profile_expiry_date { get; set; }

        public string pg_payment_date { get; set; }

        public string pg_card_hash { get; set; }

        public string pg_card_pan { get; set; }

        public string pg_need_phone_notification { get; set; }

        public string pg_user_contact_email { get; set; }

        public string pg_need_email_notification { get; set; }

        public string pg_card_exp { get; set; }

        public string pg_testing_mode { get; set; }

        public string pg_card_owner { get; set; }
    }
}