namespace IqSoft.CP.Integration.Payments.Models.PaymentAsia
{
    public  class PayoutInput
    {
        public string request_reference { get; set; }
        public string beneficiary_name { get; set; }
        public string beneficiary_first_name { get; set; }
        public string beneficiary_last_name { get; set; }
        public string bank_name { get; set; }
        public string beneficiary_email { get; set; }
        public string beneficiary_phone { get; set; }
        public string account_number { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
        public string datafeed_url { get; set; }
        public string sign { get; set; }
    }
}
