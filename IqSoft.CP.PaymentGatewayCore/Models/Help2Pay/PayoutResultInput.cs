namespace IqSoft.CP.PaymentGateway.Models.Help2Pay
{
    public class PayoutResultInput
    {
        public string MerchantCode { get; set; }

        public string TransactionID { get; set; }

        public string CurrencyCode { get; set; }

        public string Amount { get; set; }

        public string TransactionDateTime { get; set; }

        public string Key { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public string MemberCode { get; set; }

        public string ID { get; set; }
    }

    public class CheckInput
    {
        public string transId { get; set; }

        public string key { get; set; }
    }
}