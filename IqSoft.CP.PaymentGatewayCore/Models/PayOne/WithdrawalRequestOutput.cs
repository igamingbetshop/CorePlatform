using IqSoft.CP.Common.Models;

namespace IqSoft.CP.PaymentGateway.Models.PayOne
{
    public class WithdrawalRequestOutput
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long WithdrawalId { get; set; }
        public int Amount { get; set; }
        public PaymentInfo BankDetails { get; set; }
    }
}