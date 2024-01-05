namespace IqSoft.CP.PaymentGateway.Models.Help2Pay
{
    public class RequestResultInput
    {
        public string Merchant { get; set; }

        public string Reference { get; set; }

        public string Currency { get; set; }

        public string Amount { get; set; }

        public string Language { get; set; }

        public string Customer { get; set; }

        public string Datetime { get; set; }

        public string StatementDate { get; set; }

        public string Note { get; set; }

        public string Key { get; set; }

        public string Status { get; set; }

        public string ID { get; set; }
    }
}