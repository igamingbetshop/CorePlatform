namespace IqSoft.CP.Integration.Payments.Models.PayTrust88
{
    class PaymentRequestInput
    {
        public string return_url { get; set; }

        public string failed_return_url { get; set; }

        public string http_post_url { get; set; }

        public decimal amount { get; set; }

        public string currency { get; set; }

        public string item_id { get; set; }

        public string item_description { get; set; }

        public string name { get; set; }

        public string email { get; set; }

        public int descriptor { get; set; }
    }
}
