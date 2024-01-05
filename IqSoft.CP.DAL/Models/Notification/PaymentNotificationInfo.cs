namespace IqSoft.CP.DAL.Models.Notification
{
    public class PaymentNotificationInfo
    {
        public decimal Amount { get; set; }
        public int State { get; set; }
        public string Reason { get; set; }

        public string BankName { get; set; }
        public string BankBranchName { get; set; }
        public string BankCode { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountHolder { get; set; }

        public string WalletNumber { get; set; }
    }
}