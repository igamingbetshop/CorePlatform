using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.AgentModels
{
    public class CommissionItem
    {
        public int? RecieverAgentId { get; set; }
        public int? RecieverClientId { get; set; }
        public int ProductId { get; set; }
        public int SenderAgentId { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal TotalWinAmount { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalBetsCount { get; set; }
        public int TotalUnsettledBetsCount { get; set; }
        public int TotalDeletedBetsCount { get; set; }
    }
}
