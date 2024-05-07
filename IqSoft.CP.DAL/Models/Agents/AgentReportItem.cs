using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Agents
{
    public class AgentReportItem
    {
        public int AgentId { get; set; }
        public string AgentFirstName { get; set; }
        public string AgentLastName { get; set; }
        public string AgentUserName { get; set; }
        public int TotalDepositCount { get; set; }
        public int TotalWithdrawCount { get; set; }
        public decimal TotalDepositAmount { get; set; }
        public decimal TotalWithdrawAmount { get; set; }
        public int TotalBetsCount { get; set; }
        public int TotalUnsettledBetsCount { get; set; }
        public int TotalDeletedBetsCount { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal TotalWinAmount { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalProfitPercent { get; set; }
        public decimal TotalGGRCommission { get; set; }
        public decimal TotalTurnoverCommission { get; set; }
    }
}
