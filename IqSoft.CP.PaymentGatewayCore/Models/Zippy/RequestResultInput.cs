namespace IqSoft.CP.PaymentGateway.Models.Zippy
{
    public class RequestResultInput
    {
        public string AMOUNT { get; set; }
        public long MERCHANTREQUESTID { get; set; }
        public int CODE { get; set; }
        public string MESSAGE { get; set; }
        public string SIGN { get; set; }
        public string ZIPPYID { get; set; }
    }
}