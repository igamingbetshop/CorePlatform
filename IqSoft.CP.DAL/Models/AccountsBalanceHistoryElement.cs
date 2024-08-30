using IqSoft.CP.Common.Attributes;
using System;

namespace IqSoft.CP.DAL.Models
{
    public class AccountsBalanceHistoryElement
    {           
        public string OperationType { get; set; }
        public string AccountType { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal OperationAmount { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime OperationTime { get; set; }
        public int? BonusId { get; set; }
        public string RoundId { get; set; }
        public int? GameId { get; set; }
        public string GameName { get; set; }
        public string ProviderName { get; set; }
        public string SubProviderName { get; set; }
        public string PaymentSystemName { get; set; }

        [NotExcelProperty]
        public long TransactionId { get; set; }

        [NotExcelProperty]
        public long DocumentId { get; set; }

        [NotExcelProperty]
        public long AccountId { get; set; }
    }
}