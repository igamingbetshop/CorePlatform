namespace IqSoft.CP.PaymentGateway.Models.PaymentAsia
{
    public class PayoutRequestInput
    {
        public string batch_reference { get; set; } = "";
        public string request_reference { get; set; }
        public string token { get; set; }
        public string beneficiary_name { get; set; }
        public string bank_name { get; set; }
        public string account_number { get; set; }
        public string order_currency { get; set; }
        public string order_amount { get; set; }
        public int status { get; set; }
        public string fail_reason { get; set; }
        public string completed_time { get; set; } = "";
        public string created_time { get; set; }
        public string sign { get; set; }
    }
}