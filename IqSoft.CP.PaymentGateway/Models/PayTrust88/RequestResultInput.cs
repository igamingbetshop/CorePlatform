namespace IqSoft.CP.PaymentGateway.Models.PayTrust88
{
    public class RequestResultInput
    {
        public int contract { get; set; }
        public int apikey { get; set; }
        public int transaction { get; set; }
        public int status { get; set; }
        public string status_message { get; set; }
        public string item_id { get; set; }
        public string item_description { get; set; }
        public decimal amount { get; set; }
        public decimal total_fees { get; set; }
        public string currency { get; set; }
        public string name { get; set; }
        public string telephone { get; set; }
        public string email { get; set; }
        public string bank_name { get; set; }
        public string bank_account { get; set; }
        public int account { get; set; }
        public string signature { get; set; }
        public string created_at { get; set; }
    }
}