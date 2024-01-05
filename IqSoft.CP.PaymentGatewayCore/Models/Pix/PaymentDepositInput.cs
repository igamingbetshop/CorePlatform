namespace IqSoft.CP.PaymentGatewayCore.Models.Pix
{
    public class PaymentDepositInput
    {
        public string conciliation_id { get; set; }
        public string timestamp { get; set; }
        public string buyer_name { get; set; }
        public object description { get; set; }
        public string status { get; set; }
        public int amount { get; set; }

    }
}