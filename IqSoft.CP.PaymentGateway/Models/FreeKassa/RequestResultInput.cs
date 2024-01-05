namespace IqSoft.CP.PaymentGateway.Models.FreeKassa
{
    public class RequestResultInput
    {
        public string MERCHANT_ID { get; set; }
        public decimal AMOUNT { get; set; }
        public string intid { get; set; }
        public long MERCHANT_ORDER_ID { get; set; }
        public string CUR_ID { get; set; }
        public string SIGN { get; set; }
    }
}