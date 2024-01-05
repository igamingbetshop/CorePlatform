namespace IqSoft.CP.Integration.Payments.Models.Help2Pay
{
    class PayoutRequestInput
    {
        public string Key { get; set; }

        public string ClientIp { get; set; }

        public string ReturnURI { get; set; }

        public string MerchantCode { get; set; }

        public string TransactionID { get; set; }

        public string CurrencyCode { get; set; }

        public string MemberCode { get; set; }

        public string Amount { get; set; }

        public string TransactionDateTime { get; set; }

        public string BankCode { get; set; }

        public string toBankAccountName { get; set; }

        public string toBankAccountNumber { get; set; }
        
        //public string toBranch { get; set; }
    }
}

