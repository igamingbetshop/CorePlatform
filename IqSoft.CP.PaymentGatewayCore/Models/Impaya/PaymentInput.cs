namespace IqSoft.CP.PaymentGateway.Models.Impaya
{
    public class PaymentInput
    {
        public string version { get; set; }
        public string merchant_id { get; set; }
        public string mc_transaction_id { get; set; }
        public string transaction_id { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string payment_system { get; set; }
        public int status_id { get; set; }
        public string payment_system_status { get; set; }
        public string hash { get; set; }
        public string description { get; set; }
    }
}