namespace IqSoft.CP.PaymentGateway.Models.PayTrust88
{
    public class PayoutResultInput
    {
        public int contract { get; set; }
        public int apikey { get; set; }
        public int payout { get; set; }
        public string token { get; set; }
        public decimal amount { get; set; }
        public decimal total_fees { get; set; }
        public string currency { get; set; }
        public string name { get; set; }
        public int status { get; set; }
        public string status_message { get; set; }
        public string bank_code { get; set; }
        public string iban { get; set; }
        public string item_id { get; set; }
        public string item_description { get; set; }
        public string signature { get; set; }
        public string created_at { get; set; }
        public string authorization_date { get; set; }
        public int payment_date { get; set; }
    }
}