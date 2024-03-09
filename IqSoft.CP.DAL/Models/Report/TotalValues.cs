using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Models.Report
{
    public class TotalValues
    {
        public string CurrencyId { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public int TotalBetsCount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal? TotalPossibleWinsAmount { get; set; }
    }
}
