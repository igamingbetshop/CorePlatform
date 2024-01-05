namespace IqSoft.CP.Integration.Payments.Models.PaymentAsia
{
    public class PaymentInput
    {
        public string merchant_reference { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
        public string return_url { get; set; }
        public string customer_ip { get; set; }
        public string customer_first_name { get; set; }
        public string customer_last_name { get; set; }
        public string customer_phone { get; set; }
        public string customer_email { get; set; }
        public string network { get; set; }
        public string sign { get; set; }    }
}
