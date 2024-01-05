using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Dashboard
{
    public class DepositsInfo
    {
        public int Status { get; set; }

        public int TotalPlayersCount { get; set; }

        public List<DepositInfo> Deposits { get; set; }
    }

    public class DepositInfo
    {
        public string CurrencyId { get; set; }

        public int PaymentSystemId { get; set; }

        public string PaymentSystemName { get; set; }

        public decimal TotalAmount { get; set; }

        public int TotalDepositsCount { get; set; }

        public int TotalPlayersCount { get; set; }
    }
}