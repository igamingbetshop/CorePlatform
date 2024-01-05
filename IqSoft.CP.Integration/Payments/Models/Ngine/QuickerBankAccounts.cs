using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.Ngine
{
    public class QuickerBankAccounts
    {
        public QuickerAuthentication Authentication { get; set; }
    }

    public class QuickerAuthentication
    {
        public List<BankAccount> BankAccounts { get; set; }
        public object ErrorDescription { get; set; }
    }

    public class BankAccount
    {
        public int BankAccountID { get; set; }
        public string BankName { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string AccountName { get; set; }
        public string RoutingPaperCheck { get; set; }
        public string RoutingWire { get; set; }
    }

}

