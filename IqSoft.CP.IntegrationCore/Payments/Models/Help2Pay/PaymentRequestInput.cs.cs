namespace IqSoft.CP.Integration.Payments.Models.Help2Pay
{
    class PaymentRequestInput
    {
        public string Merchant { get; set; }

        public string Currency { get; set; }

        public string Customer { get; set; }

        public string Reference { get; set; }

        public string Key { get; set; }

        public string Amount { get; set; }

        public string Note { get; set; }

        public string Datetime { get; set; }

        public string FrontURI { get; set; }

        public string BackURI { get; set; }

        public string Bank { get; set; }

        public string Language { get; set; }

        public string ClientIP { get; set; }
    }
}
