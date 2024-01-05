namespace IqSoft.CP.Integration.Payments.Models.TronLink
{
    public class PaymentOutput
    {
        public int id { get; set; }
        public DataDetails data { get; set; }
    }

    public class DataDetails
    {
        public string address { get; set; }
        public decimal amount { get; set; }
        public string type { get; set; }
        public bool success { get; set; }
        public bool confirmed { get; set; }
    }
}