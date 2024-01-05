namespace IqSoft.CP.Integration.Payments.Models.TronLink
{
    public class PayoutOutput
    {

        public bool success { get; set; }
        public int id { get; set; }
        public PayoutDataDetails data { get; set; }
        public ErrorDetails error { get; set; }
    }

    public class PayoutDataDetails
    {
        public string tx { get; set; }
    }

    public class ErrorDetails
    {
        public string status { get; set; }
        public string message { get; set; }
    }
}