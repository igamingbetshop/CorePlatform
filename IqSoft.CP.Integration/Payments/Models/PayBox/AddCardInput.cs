namespace IqSoft.CP.Integration.Payments.Models.PayBox
{
    public class AddCardInput
    {
        public string pg_merchant_id { get; set; }

        public int pg_user_id { get; set; }

        public long pg_order_id { get; set; }

        public string pg_post_link { get; set; }

        public string pg_back_link { get; set; }

        public string pg_salt { get; set; }

        public string pg_sig { get; set; }
    }
}
