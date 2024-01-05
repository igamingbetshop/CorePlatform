namespace IqSoft.CP.Integration.Payments.Models.PayTrust88
{
    class PayoutRequestInput
    {
        public decimal amount { get; set; }

        public string currency { get; set; }

        public string name { get; set; }

        public string bank_name { get; set; }
        public string bank_code { get; set; }

        public string iban { get; set; }

        public string http_post_url { get; set; }

        public string item_id { get; set; }

        public string item_description { get; set; }
    }
}
