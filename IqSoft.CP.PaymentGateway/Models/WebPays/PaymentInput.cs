namespace IqSoft.CP.PaymentGateway.Models.WebPays
{
    public class PaymentInput
    {
        public string transID { get; set; }
        public int order_status { get; set; }
        public string status { get; set; }
        public decimal bill_amt { get; set; }
        public string descriptor { get; set; }
        public string tdate { get; set; }
        public string bill_currency { get; set; }
        public string response { get; set; }
        public string reference { get; set; }
    }
}