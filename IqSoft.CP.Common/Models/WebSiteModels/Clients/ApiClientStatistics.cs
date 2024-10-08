using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class ApiClientStatistics
    {
        public string Username { get; set; }
        public DateTime CreationTime { get; set; }
        public int VIPLevel { get; set; }
        public decimal TotalBetAmount { get; set; }
        public int TotalBetCount { get; set; }
        public int TotalWinCount { get; set; }
        public int TotalLossCount { get; set; }
        public decimal SportBetAmount { get; set; }
        public int SportBetCount { get; set; }
        public int SportWinCount { get; set; }
        public int SportLossCount { get; set; }
    }
}
