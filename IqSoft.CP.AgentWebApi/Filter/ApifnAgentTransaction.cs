using System;

namespace IqSoft.CP.AgentWebApi.Filters
{
    public class ApifnAgentTransaction
    {
        public long Id { get; set; }
        public string ExternalTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
        public int State { get; set; }
        public int OperationTypeId { get; set; }
        public int? FromUserId { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public string TransactionType { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}