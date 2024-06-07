using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Dashboard
{
    public class DashboardBonusInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PartnerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public int Type { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}
