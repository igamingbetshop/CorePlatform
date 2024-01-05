using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.CommonCore.Models.WebSiteModels
{
    public class LimitInfo
    {
        public int ClientId { get; set; }
        public int? DailyDepositLimitPercent { get; set; }
        public int? WeeklyDepositLimitPercent { get; set; }
        public int? MonthlyDepositLimitPercent { get; set; }
        public int? DailyBetLimitPercent { get; set; }
        public int? WeeklyBetLimitPercent { get; set; }
        public int? MonthlyBetLimitPercent { get; set; }
    }
}
