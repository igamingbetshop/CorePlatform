namespace IqSoft.CP.PaymentGatewayCore.Models.Pix
{
    public class PaymentWithdrawInput
    {  
        public string uuid { get; set; }
        public int amount { get; set; }
        public string scheduling_date { get; set; }
        public string confirmed_at { get; set; }
        public string status { get; set; }
    }
}