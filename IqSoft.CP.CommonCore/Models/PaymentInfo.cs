namespace IqSoft.CP.Common.Models
{
    public class PaymentInfo
    {
        public string WalletNumber { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string SMSCode { get; set; }
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string CardType { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int? CountryId { get; set; }
        public string BillingAddress { get; set; }
        public string TransactionIp { get; set; }
        public string Domain { get; set; }
        public string AccountType { get; set; }
        public string TypeDocumentId { get; set; }
        public string ExpiryDate { get; set; }
        public string ExpirationDate { get; set; }
        public string BankId { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountHolder { get; set; }
        public string BankACH { get; set; }
        public string BankBranchName { get; set; }
        public string BeneficiaryName { get; set; }
        public string BankIBAN { get; set; }
        public string SwiftCode { get; set; }
        public string NameSurname { get; set; }
        public string NationalId { get; set; }
        public string BeneficiaryID { get; set; }
        public string Info { get; set; }
        public string VoucherNumber { get; set; }
        public string ActivationCode { get; set; }
        public string VoucherNum { get; set; }
        public string VoucherCode { get; set; }
        public string VoucherAmount { get; set; }
        public string BatchNumber { get; set; }
        public string TrackingNumber { get; set; }
        public string InvoiceId { get; set; }
        public string TxnHash { get; set; }
        public string BlockHash { get; set; }
        public string BlockNumber { get; set; }
        public string PayerAccount { get; set; }
        public string PayeeAccount { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal? PayAmount { get; set; }        
        public decimal Amount { get; set; }        
    }
}
