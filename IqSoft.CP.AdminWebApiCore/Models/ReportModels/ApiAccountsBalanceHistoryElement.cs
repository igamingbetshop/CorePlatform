using System;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels
{
    public class ApiAccountsBalanceHistoryElement
    {
        public long TransactionId { get; set; }

        public long DocumentId { get; set; }

        public long AccountId { get; set; }

        public string AccountType { get; set; }

        public decimal BalanceBefore { get; set; }

        public string OperationType { get; set; }

        public decimal OperationAmount { get; set; }

        public decimal BalanceAfter { get; set; }

        public DateTime OperationTime { get; set; } 
        public string PaymentSystemName { get; set; }
    }
}