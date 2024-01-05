using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Dashboard
{
    public class WithdrawalsInfo
    {
        public int Status { get; set; }

        public int TotalPlayersCount { get; set; }

        public List<WithdrawalInfo> Withdrawals { get; set; }
    }

    public class WithdrawalInfo
    {
        public string CurrencyId { get; set; }

        public int PaymentSystemId { get; set; }

        public string PaymentSystemName { get; set; }

        public decimal TotalAmount { get; set; }

        public int TotalWithdrawalsCount { get; set; }

        public int TotalPlayersCount { get; set; }
    }
}