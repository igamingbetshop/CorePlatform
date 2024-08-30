namespace IqSoft.CP.DAL.Models.Notification
{
    public class Composite
    {
        public string VerificationCode { get; set; }
        public string Domain { get; set; }
        public int? ClientId { get; set; }
        public int? AffiliateId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public int? PaymentRequestState { get; set; }
        public string Parameters { get; set; }
        public decimal? Amount { get; set; }

        public string BankName { get; set; }
        public string BankBranchName { get; set; }
        public string BankCode { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountHolder { get; set; }

        public string Currency { get; set; }
        public string Reason { get; set; }
        public string MessageText { get; set; }
    }
}