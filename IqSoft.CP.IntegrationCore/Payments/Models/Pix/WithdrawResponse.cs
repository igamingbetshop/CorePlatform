namespace IqSoft.CP.IntegrationCore.Payments.Models.Pix
{
    public class WithdrawResponse
    {
        public string uuid { get; set; }
        public string description { get; set; }
        public string recipient_document { get; set; }
        public string recipient_name { get; set; }
        public string address_key { get; set; }
        public double amount { get; set; }
        public string status { get; set; }
    }
}
